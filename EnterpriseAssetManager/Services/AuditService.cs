using EnterpriseAssetManager.Data;
using EnterpriseAssetManager.Models;
using EnterpriseAssetManager.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAssetManager.Services;

public interface IAuditService
{
    Task LogAsync(int? userId, string action, string entityName, int? entityId, string details);
    Task<PagedResult<AuditLog>> GetPagedAsync(string search, int page, int pageSize);
}

// Central place to record important actions. Keeping this in one service means
// every controller logs audit entries the same way.
public class AuditService : IAuditService
{
    private readonly ApplicationDbContext _db;

    public AuditService(ApplicationDbContext db)
    {
        _db = db;
    }

    public async Task LogAsync(int? userId, string action, string entityName, int? entityId, string details)
    {
        _db.AuditLogs.Add(new AuditLog
        {
            UserId = userId,
            Action = action,
            EntityName = entityName,
            EntityId = entityId,
            Details = details,
            CreatedAt = DateTime.UtcNow
        });

        await _db.SaveChangesAsync();
    }

    public async Task<PagedResult<AuditLog>> GetPagedAsync(string search, int page, int pageSize)
    {
        IQueryable<AuditLog> query = _db.AuditLogs
            .AsNoTracking()
            .Include(a => a.User);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(a =>
                a.Action.Contains(search) ||
                a.EntityName.Contains(search) ||
                a.Details.Contains(search) ||
                (a.User != null && a.User.FullName.Contains(search)));
        }

        return await query.OrderByDescending(a => a.CreatedAt).ToPagedResultAsync(page, pageSize);
    }
}
