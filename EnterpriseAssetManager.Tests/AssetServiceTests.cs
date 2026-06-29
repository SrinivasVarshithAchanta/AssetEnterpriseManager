using EnterpriseAssetManager.Models;
using Xunit;

namespace EnterpriseAssetManager.Tests;

public class AssetServiceTests
{
    [Fact]
    public async Task AssetTagExists_ReturnsTrue_WhenTagPresent()
    {
        using var db = TestSupport.NewDb();
        var category = await TestSupport.AddCategoryAsync(db);
        await TestSupport.AddAssetAsync(db, category.Id, "AST-1001");
        var assets = TestSupport.Assets(db);

        Assert.True(await assets.AssetTagExistsAsync("AST-1001"));
        Assert.False(await assets.AssetTagExistsAsync("AST-9999"));
    }

    [Fact]
    public async Task Create_Throws_OnDuplicateTag()
    {
        using var db = TestSupport.NewDb();
        var category = await TestSupport.AddCategoryAsync(db);
        await TestSupport.AddAssetAsync(db, category.Id, "AST-1001");
        var assets = TestSupport.Assets(db);

        var duplicate = new Asset
        {
            AssetTag = "AST-1001",
            Name = "Another",
            CategoryId = category.Id,
            SerialNumber = "SN-x",
            PurchaseDate = DateTime.Today
        };

        await Assert.ThrowsAsync<InvalidOperationException>(() => assets.CreateAsync(duplicate, null));
    }

    [Fact]
    public async Task Retire_SetsStatusToRetired()
    {
        using var db = TestSupport.NewDb();
        var category = await TestSupport.AddCategoryAsync(db);
        var asset = await TestSupport.AddAssetAsync(db, category.Id, "AST-2002");
        var assets = TestSupport.Assets(db);

        var ok = await assets.RetireAsync(asset.Id, null);

        Assert.True(ok);
        var reloaded = await assets.GetByIdAsync(asset.Id);
        Assert.Equal(AssetStatus.Retired, reloaded.Status);
    }

    [Fact]
    public async Task GetPaged_FiltersBySearchTerm()
    {
        using var db = TestSupport.NewDb();
        var category = await TestSupport.AddCategoryAsync(db);
        await TestSupport.AddAssetAsync(db, category.Id, "AST-3001");
        await TestSupport.AddAssetAsync(db, category.Id, "AST-3002");
        await TestSupport.AddAssetAsync(db, category.Id, "AST-4001");
        var assets = TestSupport.Assets(db);

        var page = await assets.GetPagedAsync("AST-30", null, null, null, 1, 10);

        Assert.Equal(2, page.TotalCount);
    }

    [Fact]
    public async Task CountAvailableByCategory_CountsOnlyAvailable()
    {
        using var db = TestSupport.NewDb();
        var category = await TestSupport.AddCategoryAsync(db);
        await TestSupport.AddAssetAsync(db, category.Id, "AST-5001", AssetStatus.Available);
        await TestSupport.AddAssetAsync(db, category.Id, "AST-5002", AssetStatus.Available);
        await TestSupport.AddAssetAsync(db, category.Id, "AST-5003", AssetStatus.Retired);
        var assets = TestSupport.Assets(db);

        Assert.Equal(2, await assets.CountAvailableByCategoryAsync(category.Id));
    }
}
