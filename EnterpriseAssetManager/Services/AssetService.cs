using EnterpriseAssetManager.Data;
using EnterpriseAssetManager.Models;
using EnterpriseAssetManager.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAssetManager.Services;

public interface IAssetService
{
    Task<PagedResult<Asset>> GetPagedAsync(string search, int? categoryId, AssetStatus? status, AssetCondition? condition, int page, int pageSize);
    Task<Asset> GetByIdAsync(int id);
    Task<bool> AssetTagExistsAsync(string assetTag, int? excludeAssetId = null);
    Task<Asset> CreateAsync(Asset asset, int? actingUserId);
    Task<bool> UpdateAsync(Asset asset, int? actingUserId);
    Task<bool> RetireAsync(int assetId, int? actingUserId);

    Task<List<Asset>> GetAvailableByCategoryAsync(int categoryId);
    Task<int> CountAvailableByCategoryAsync(int categoryId);
    Task<AssetStats> GetStatsAsync();

    // Category management is small enough to live alongside assets.
    Task<List<AssetCategory>> GetAllCategoriesAsync();
    Task<List<AssetCategory>> GetActiveCategoriesAsync();
    Task<AssetCategory> GetCategoryByIdAsync(int id);
    Task<bool> CategoryNameExistsAsync(string name, int? excludeCategoryId = null);
    Task<AssetCategory> CreateCategoryAsync(AssetCategory category, int? actingUserId);
    Task<bool> UpdateCategoryAsync(AssetCategory category, int? actingUserId);
    Task<bool> SetCategoryActiveAsync(int categoryId, bool isActive, int? actingUserId);
}

public class AssetService : IAssetService
{
    private readonly ApplicationDbContext _db;
    private readonly IAuditService _audit;

    public AssetService(ApplicationDbContext db, IAuditService audit)
    {
        _db = db;
        _audit = audit;
    }

    public async Task<PagedResult<Asset>> GetPagedAsync(
        string search, int? categoryId, AssetStatus? status, AssetCondition? condition, int page, int pageSize)
    {
        IQueryable<Asset> query = _db.Assets
            .AsNoTracking()
            .Include(a => a.Category)
            .Include(a => a.AssignedToUser);

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(a =>
                a.AssetTag.Contains(search) ||
                a.Name.Contains(search) ||
                a.SerialNumber.Contains(search));
        }

        if (categoryId.HasValue)
            query = query.Where(a => a.CategoryId == categoryId.Value);

        if (status.HasValue)
            query = query.Where(a => a.Status == status.Value);

        if (condition.HasValue)
            query = query.Where(a => a.Condition == condition.Value);

        return await query.OrderBy(a => a.AssetTag).ToPagedResultAsync(page, pageSize);
    }

    public Task<Asset> GetByIdAsync(int id) =>
        _db.Assets
            .Include(a => a.Category)
            .Include(a => a.AssignedToUser)
            .FirstOrDefaultAsync(a => a.Id == id);

    public Task<bool> AssetTagExistsAsync(string assetTag, int? excludeAssetId = null)
    {
        assetTag = (assetTag ?? string.Empty).Trim();
        return _db.Assets.AnyAsync(a => a.AssetTag == assetTag && (!excludeAssetId.HasValue || a.Id != excludeAssetId.Value));
    }

    public async Task<Asset> CreateAsync(Asset asset, int? actingUserId)
    {
        if (await AssetTagExistsAsync(asset.AssetTag))
            throw new InvalidOperationException("An asset with this tag already exists.");

        asset.AssetTag = asset.AssetTag.Trim();
        asset.CreatedAt = DateTime.UtcNow;

        _db.Assets.Add(asset);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(actingUserId, "CreateAsset", "Asset", asset.Id, $"Created asset {asset.AssetTag}.");
        return asset;
    }

    public async Task<bool> UpdateAsync(Asset asset, int? actingUserId)
    {
        var existing = await _db.Assets.FirstOrDefaultAsync(a => a.Id == asset.Id);
        if (existing == null)
            return false;

        existing.Name = asset.Name;
        existing.CategoryId = asset.CategoryId;
        existing.SerialNumber = asset.SerialNumber;
        existing.Status = asset.Status;
        existing.Condition = asset.Condition;
        existing.PurchaseDate = asset.PurchaseDate;
        existing.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _audit.LogAsync(actingUserId, "UpdateAsset", "Asset", existing.Id, $"Updated asset {existing.AssetTag}.");
        return true;
    }

    public async Task<bool> RetireAsync(int assetId, int? actingUserId)
    {
        var asset = await _db.Assets.FirstOrDefaultAsync(a => a.Id == assetId);
        if (asset == null)
            return false;

        asset.Status = AssetStatus.Retired;
        asset.AssignedToUserId = null;
        asset.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
        await _audit.LogAsync(actingUserId, "RetireAsset", "Asset", asset.Id, $"Retired asset {asset.AssetTag}.");
        return true;
    }

    public Task<List<Asset>> GetAvailableByCategoryAsync(int categoryId) =>
        _db.Assets
            .AsNoTracking()
            .Where(a => a.CategoryId == categoryId && a.Status == AssetStatus.Available)
            .OrderBy(a => a.AssetTag)
            .ToListAsync();

    public Task<int> CountAvailableByCategoryAsync(int categoryId) =>
        _db.Assets.CountAsync(a => a.CategoryId == categoryId && a.Status == AssetStatus.Available);

    public async Task<AssetStats> GetStatsAsync()
    {
        var assets = _db.Assets.AsNoTracking();
        return new AssetStats
        {
            Total = await assets.CountAsync(),
            Available = await assets.CountAsync(a => a.Status == AssetStatus.Available),
            Assigned = await assets.CountAsync(a => a.Status == AssetStatus.Assigned),
            UnderMaintenance = await assets.CountAsync(a => a.Status == AssetStatus.UnderMaintenance),
            Retired = await assets.CountAsync(a => a.Status == AssetStatus.Retired)
        };
    }

    // ---------- Categories ----------

    public Task<List<AssetCategory>> GetAllCategoriesAsync() =>
        _db.AssetCategories.AsNoTracking().OrderBy(c => c.Name).ToListAsync();

    public Task<List<AssetCategory>> GetActiveCategoriesAsync() =>
        _db.AssetCategories.AsNoTracking().Where(c => c.IsActive).OrderBy(c => c.Name).ToListAsync();

    public Task<AssetCategory> GetCategoryByIdAsync(int id) =>
        _db.AssetCategories.FirstOrDefaultAsync(c => c.Id == id);

    public Task<bool> CategoryNameExistsAsync(string name, int? excludeCategoryId = null)
    {
        name = (name ?? string.Empty).Trim();
        return _db.AssetCategories.AnyAsync(c => c.Name == name && (!excludeCategoryId.HasValue || c.Id != excludeCategoryId.Value));
    }

    public async Task<AssetCategory> CreateCategoryAsync(AssetCategory category, int? actingUserId)
    {
        if (await CategoryNameExistsAsync(category.Name))
            throw new InvalidOperationException("A category with this name already exists.");

        category.Name = category.Name.Trim();
        _db.AssetCategories.Add(category);
        await _db.SaveChangesAsync();

        await _audit.LogAsync(actingUserId, "CreateCategory", "AssetCategory", category.Id, $"Created category {category.Name}.");
        return category;
    }

    public async Task<bool> UpdateCategoryAsync(AssetCategory category, int? actingUserId)
    {
        var existing = await _db.AssetCategories.FirstOrDefaultAsync(c => c.Id == category.Id);
        if (existing == null)
            return false;

        existing.Name = category.Name;
        existing.Description = category.Description;
        existing.IsActive = category.IsActive;

        await _db.SaveChangesAsync();
        await _audit.LogAsync(actingUserId, "UpdateCategory", "AssetCategory", existing.Id, $"Updated category {existing.Name}.");
        return true;
    }

    public async Task<bool> SetCategoryActiveAsync(int categoryId, bool isActive, int? actingUserId)
    {
        var category = await _db.AssetCategories.FirstOrDefaultAsync(c => c.Id == categoryId);
        if (category == null)
            return false;

        category.IsActive = isActive;
        await _db.SaveChangesAsync();
        await _audit.LogAsync(actingUserId, isActive ? "ActivateCategory" : "DeactivateCategory", "AssetCategory", category.Id, $"Category {category.Name} active set to {isActive}.");
        return true;
    }
}
