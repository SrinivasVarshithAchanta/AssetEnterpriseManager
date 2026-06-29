using EnterpriseAssetManager.Data;
using EnterpriseAssetManager.Models;
using EnterpriseAssetManager.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAssetManager.Services;

public interface IRequestService
{
    Task<string> GenerateRequestNumberAsync();
    Task<AssetRequest> CreateAsync(int requestedByUserId, int categoryId, string businessReason, RequestPriority priority);

    Task<PagedResult<AssetRequest>> GetMyRequestsAsync(int userId, RequestStatus? status, int page, int pageSize);
    Task<PagedResult<AssetRequest>> GetForApprovalAsync(RequestStatus? status, int? categoryId, RequestPriority? priority, DateTime? fromDate, DateTime? toDate, int page, int pageSize);
    Task<AssetRequest> GetByIdAsync(int id);

    Task<bool> CancelAsync(int requestId, int actingUserId);
    Task<bool> ApproveAsync(int requestId, int reviewerUserId, string comments);
    Task<bool> RejectAsync(int requestId, int reviewerUserId, string comments);
    Task<bool> FulfillAsync(int requestId, int assetId, int actingUserId);

    Task<RequestStats> GetStatsForUserAsync(int userId);
    Task<RequestStats> GetGlobalStatsAsync();
    Task<List<AssetRequest>> GetRecentlyReviewedAsync(int count);
}

public class RequestService : IRequestService
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;

    public RequestService(ApplicationDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<string> GenerateRequestNumberAsync()
    {
        int year = DateTime.UtcNow.Year;
        string prefix = $"REQ-{year}-";

        // Find the highest existing number for this year and increment it.
        string last = await _db.AssetRequests
            .Where(r => r.RequestNumber.StartsWith(prefix))
            .OrderByDescending(r => r.RequestNumber)
            .Select(r => r.RequestNumber)
            .FirstOrDefaultAsync();

        int next = 1;
        if (last != null && int.TryParse(last.Substring(prefix.Length), out int parsed))
            next = parsed + 1;

        return prefix + next.ToString("000000");
    }

    public async Task<AssetRequest> CreateAsync(int requestedByUserId, int categoryId, string businessReason, RequestPriority priority)
    {
        var request = new AssetRequest
        {
            RequestNumber = await GenerateRequestNumberAsync(),
            RequestedByUserId = requestedByUserId,
            AssetCategoryId = categoryId,
            BusinessReason = businessReason,
            Priority = priority,
            Status = RequestStatus.Pending,
            RequestedAt = DateTime.UtcNow
        };

        _db.AssetRequests.Add(request);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(requestedByUserId, "CreateRequest", "AssetRequest", request.Id, $"Created request {request.RequestNumber}.");
        return request;
    }

    public async Task<PagedResult<AssetRequest>> GetMyRequestsAsync(int userId, RequestStatus? status, int page, int pageSize)
    {
        IQueryable<AssetRequest> query = _db.AssetRequests
            .AsNoTracking()
            .Include(r => r.AssetCategory)
            .Include(r => r.Asset)
            .Where(r => r.RequestedByUserId == userId);

        if (status.HasValue)
            query = query.Where(r => r.Status == status.Value);

        return await query.OrderByDescending(r => r.RequestedAt).ToPagedResultAsync(page, pageSize);
    }

    public async Task<PagedResult<AssetRequest>> GetForApprovalAsync(
        RequestStatus? status, int? categoryId, RequestPriority? priority, DateTime? fromDate, DateTime? toDate, int page, int pageSize)
    {
        IQueryable<AssetRequest> query = _db.AssetRequests
            .AsNoTracking()
            .Include(r => r.RequestedByUser)
            .Include(r => r.AssetCategory)
            .Include(r => r.Asset);

        // Default the approvals queue to pending unless a specific status is chosen.
        query = status.HasValue
            ? query.Where(r => r.Status == status.Value)
            : query.Where(r => r.Status == RequestStatus.Pending);

        if (categoryId.HasValue)
            query = query.Where(r => r.AssetCategoryId == categoryId.Value);

        if (priority.HasValue)
            query = query.Where(r => r.Priority == priority.Value);

        if (fromDate.HasValue)
            query = query.Where(r => r.RequestedAt >= fromDate.Value);

        if (toDate.HasValue)
        {
            var inclusiveEnd = toDate.Value.Date.AddDays(1);
            query = query.Where(r => r.RequestedAt < inclusiveEnd);
        }

        return await query.OrderByDescending(r => r.RequestedAt).ToPagedResultAsync(page, pageSize);
    }

    public Task<AssetRequest> GetByIdAsync(int id) =>
        _db.AssetRequests
            .AsNoTracking()
            .Include(r => r.RequestedByUser)
            .Include(r => r.AssetCategory)
            .Include(r => r.Asset)
            .Include(r => r.ReviewedByUser)
            .FirstOrDefaultAsync(r => r.Id == id);

    public async Task<bool> CancelAsync(int requestId, int actingUserId)
    {
        var request = await _db.AssetRequests.FirstOrDefaultAsync(r => r.Id == requestId);
        if (request == null)
            return false;

        if (request.RequestedByUserId != actingUserId)
            throw new UnauthorizedAccessException("You can only cancel your own requests.");

        if (request.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be cancelled.");

        request.Status = RequestStatus.Cancelled;
        await _db.SaveChangesAsync();

        await _audit.LogAsync(actingUserId, "CancelRequest", "AssetRequest", request.Id, $"Cancelled request {request.RequestNumber}.");
        return true;
    }

    public async Task<bool> ApproveAsync(int requestId, int reviewerUserId, string comments)
    {
        var request = await _db.AssetRequests.FirstOrDefaultAsync(r => r.Id == requestId);
        if (request == null)
            return false;

        if (request.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be approved.");

        request.Status = RequestStatus.Approved;
        request.ReviewedByUserId = reviewerUserId;
        request.ReviewedAt = DateTime.UtcNow;
        request.ManagerComments = string.IsNullOrWhiteSpace(comments) ? null : comments.Trim();

        await _db.SaveChangesAsync();
        await _audit.LogAsync(reviewerUserId, "ApproveRequest", "AssetRequest", request.Id, $"Approved request {request.RequestNumber}.");
        return true;
    }

    public async Task<bool> RejectAsync(int requestId, int reviewerUserId, string comments)
    {
        // A manager comment is mandatory when rejecting.
        if (string.IsNullOrWhiteSpace(comments))
            throw new ArgumentException("A comment is required when rejecting a request.", nameof(comments));

        var request = await _db.AssetRequests.FirstOrDefaultAsync(r => r.Id == requestId);
        if (request == null)
            return false;

        if (request.Status != RequestStatus.Pending)
            throw new InvalidOperationException("Only pending requests can be rejected.");

        request.Status = RequestStatus.Rejected;
        request.ReviewedByUserId = reviewerUserId;
        request.ReviewedAt = DateTime.UtcNow;
        request.ManagerComments = comments.Trim();

        await _db.SaveChangesAsync();
        await _audit.LogAsync(reviewerUserId, "RejectRequest", "AssetRequest", request.Id, $"Rejected request {request.RequestNumber}.");
        return true;
    }

    public async Task<bool> FulfillAsync(int requestId, int assetId, int actingUserId)
    {
        var request = await _db.AssetRequests.FirstOrDefaultAsync(r => r.Id == requestId);
        if (request == null)
            return false;

        if (request.Status != RequestStatus.Approved)
            throw new InvalidOperationException("Only approved requests can be fulfilled.");

        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == assetId);
        if (asset == null)
            throw new InvalidOperationException("The selected asset does not exist.");

        if (asset.Status == AssetStatus.Retired)
            throw new InvalidOperationException("Retired assets cannot be assigned.");

        if (asset.Status != AssetStatus.Available)
            throw new InvalidOperationException("Only available assets can be assigned.");

        // Assign the asset to the requester and mark it as Assigned.
        asset.AssignedToUserId = request.RequestedByUserId;
        asset.Status = AssetStatus.Assigned;
        asset.UpdatedAt = DateTime.UtcNow;

        request.AssetId = asset.Id;
        request.Status = RequestStatus.Fulfilled;

        await _db.SaveChangesAsync();
        await _audit.LogAsync(actingUserId, "FulfillRequest", "AssetRequest", request.Id, $"Fulfilled request {request.RequestNumber} with asset {asset.AssetTag}.");
        return true;
    }

    public async Task<RequestStats> GetStatsForUserAsync(int userId)
    {
        var query = _db.AssetRequests.AsNoTracking().Where(r => r.RequestedByUserId == userId);
        return await BuildStatsAsync(query);
    }

    public async Task<RequestStats> GetGlobalStatsAsync()
    {
        var query = _db.AssetRequests.AsNoTracking();
        return await BuildStatsAsync(query);
    }

    public Task<List<AssetRequest>> GetRecentlyReviewedAsync(int count) =>
        _db.AssetRequests
            .AsNoTracking()
            .Include(r => r.RequestedByUser)
            .Include(r => r.AssetCategory)
            .Where(r => r.Status == RequestStatus.Approved || r.Status == RequestStatus.Rejected || r.Status == RequestStatus.Fulfilled)
            .OrderByDescending(r => r.ReviewedAt)
            .Take(count)
            .ToListAsync();

    private static async Task<RequestStats> BuildStatsAsync(IQueryable<AssetRequest> query)
    {
        return new RequestStats
        {
            Total = await query.CountAsync(),
            Pending = await query.CountAsync(r => r.Status == RequestStatus.Pending),
            Approved = await query.CountAsync(r => r.Status == RequestStatus.Approved),
            Rejected = await query.CountAsync(r => r.Status == RequestStatus.Rejected),
            Fulfilled = await query.CountAsync(r => r.Status == RequestStatus.Fulfilled),
            Cancelled = await query.CountAsync(r => r.Status == RequestStatus.Cancelled)
        };
    }
}
