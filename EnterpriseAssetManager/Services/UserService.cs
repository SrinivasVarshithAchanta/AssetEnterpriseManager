using EnterpriseAssetManager.Data;
using EnterpriseAssetManager.Models;
using EnterpriseAssetManager.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAssetManager.Services;

public interface IUserService
{
    Task<PagedResult<User>> GetPagedAsync(string search, string role, bool? isActive, int page, int pageSize);
    Task<User> GetByIdAsync(int id);
    Task<bool> EmailExistsAsync(string email, int? excludeUserId = null);
    Task<User> CreateAsync(User user, string password, int? actingUserId);
    Task<bool> UpdateAsync(User user, int? actingUserId);
    Task<bool> SetActiveAsync(int userId, bool isActive, int? actingUserId);
}

public class UserService : IUserService
{
    private readonly ApplicationDbContext _db;
    private readonly IPasswordHasher _hasher;
    private readonly IAuditService _audit;

    public UserService(ApplicationDbContext db, IPasswordHasher hasher, IAuditService audit)
    {
        _db = db;
        _hasher = hasher;
        _audit = audit;
    }

    public async Task<PagedResult<User>> GetPagedAsync(string search, string role, bool? isActive, int page, int pageSize)
    {
        // Build the query server side. Nothing is materialised until ToListAsync,
        // so SQL Server does the filtering, paging and counting, not the web server.
        IQueryable<User> query = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(u => u.FullName.Contains(search) || u.Email.Contains(search));
        }

        if (!string.IsNullOrWhiteSpace(role))
            query = query.Where(u => u.Role == role);

        if (isActive.HasValue)
            query = query.Where(u => u.IsActive == isActive.Value);

        return await PaginateAsync(query.OrderBy(u => u.FullName), page, pageSize);
    }

    public Task<User> GetByIdAsync(int id) =>
        _db.Users.FirstOrDefaultAsync(u => u.Id == id);

    public Task<bool> EmailExistsAsync(string email, int? excludeUserId = null)
    {
        email = (email ?? string.Empty).Trim();
        return _db.Users.AnyAsync(u => u.Email == email && (!excludeUserId.HasValue || u.Id != excludeUserId.Value));
    }

    public async Task<User> CreateAsync(User user, string password, int? actingUserId)
    {
        if (await EmailExistsAsync(user.Email))
            throw new InvalidOperationException("A user with this email already exists.");

        user.Email = user.Email.Trim();
        user.PasswordHash = _hasher.Hash(password);
        user.CreatedAt = DateTime.UtcNow;

        _db.Users.Add(user);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(actingUserId, "CreateUser", "User", user.Id, $"Created user {user.Email} with role {user.Role}.");
        return user;
    }

    public async Task<bool> UpdateAsync(User user, int? actingUserId)
    {
        var existing = await _db.Users.FirstOrDefaultAsync(u => u.Id == user.Id);
        if (existing == null)
            return false;

        existing.FullName = user.FullName;
        existing.Department = user.Department;
        existing.Role = user.Role;
        existing.IsActive = user.IsActive;

        await _db.SaveChangesAsync();
        await _audit.LogAsync(actingUserId, "UpdateUser", "User", existing.Id, $"Updated user {existing.Email}.");
        return true;
    }

    public async Task<bool> SetActiveAsync(int userId, bool isActive, int? actingUserId)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == userId);
        if (user == null)
            return false;

        user.IsActive = isActive;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(actingUserId, isActive ? "ActivateUser" : "DeactivateUser", "User", user.Id, $"User {user.Email} active set to {isActive}.");
        return true;
    }

    private static async Task<PagedResult<User>> PaginateAsync(IQueryable<User> query, int page, int pageSize)
    {
        if (page < 1) page = 1;
        if (pageSize < 1) pageSize = 10;

        int total = await query.CountAsync();
        var items = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return new PagedResult<User>
        {
            Items = items,
            Page = page,
            PageSize = pageSize,
            TotalCount = total
        };
    }
}
