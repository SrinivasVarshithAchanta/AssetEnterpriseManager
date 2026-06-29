using EnterpriseAssetManager.Data;
using EnterpriseAssetManager.Models;
using EnterpriseAssetManager.Services;
using Microsoft.EntityFrameworkCore;

namespace EnterpriseAssetManager.Tests;

// Helpers to spin up an isolated in-memory database and wire the real services
// for each test. The in-memory provider does not enforce relational constraints,
// so uniqueness rules are tested through the service logic that owns them.
public static class TestSupport
{
    public static ApplicationDbContext NewDb()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .EnableSensitiveDataLogging()
            .Options;

        return new ApplicationDbContext(options);
    }

    public static AuditService Audit(ApplicationDbContext db) => new(db);
    public static PasswordHasher Hasher() => new();
    public static AssetService Assets(ApplicationDbContext db) => new(db, Audit(db));
    public static RequestService Requests(ApplicationDbContext db) => new(db, Audit(db));
    public static UserService Users(ApplicationDbContext db) => new(db, Hasher(), Audit(db));
    public static AuthService Auth(ApplicationDbContext db) => new(db, Hasher(), Audit(db));

    public static async Task<User> AddUserAsync(
        ApplicationDbContext db, string email, string password, string role, bool isActive = true)
    {
        var user = new User
        {
            FullName = email.Split('@')[0],
            Email = email,
            PasswordHash = Hasher().Hash(password),
            Role = role,
            Department = "Engineering",
            IsActive = isActive
        };
        db.Users.Add(user);
        await db.SaveChangesAsync();
        return user;
    }

    public static async Task<AssetCategory> AddCategoryAsync(ApplicationDbContext db, string name = "Laptop")
    {
        var category = new AssetCategory { Name = name, Description = name, IsActive = true };
        db.AssetCategories.Add(category);
        await db.SaveChangesAsync();
        return category;
    }

    public static async Task<Asset> AddAssetAsync(
        ApplicationDbContext db, int categoryId, string tag, AssetStatus status = AssetStatus.Available)
    {
        var asset = new Asset
        {
            AssetTag = tag,
            Name = tag + " device",
            CategoryId = categoryId,
            SerialNumber = "SN-" + tag,
            Status = status,
            Condition = AssetCondition.Good,
            PurchaseDate = DateTime.Today.AddMonths(-3)
        };
        db.Assets.Add(asset);
        await db.SaveChangesAsync();
        return asset;
    }
}
