using EnterpriseAssetManager.Models;
using Xunit;

namespace EnterpriseAssetManager.Tests;

public class PaginationTests
{
    [Fact]
    public async Task AssetService_GetPaged_ReturnsCorrectPageAndTotals()
    {
        using var db = TestSupport.NewDb();
        var category = await TestSupport.AddCategoryAsync(db);
        var assets = TestSupport.Assets(db);

        for (int i = 1; i <= 25; i++)
            await TestSupport.AddAssetAsync(db, category.Id, $"AST-P{i:000}");

        var page1 = await assets.GetPagedAsync(null, null, null, null, page: 1, pageSize: 10);
        var page2 = await assets.GetPagedAsync(null, null, null, null, page: 2, pageSize: 10);
        var page3 = await assets.GetPagedAsync(null, null, null, null, page: 3, pageSize: 10);

        Assert.Equal(25, page1.TotalCount);
        Assert.Equal(10, page1.Items.Count);
        Assert.Equal(1, page1.Page);
        Assert.True(page1.HasNext);
        Assert.False(page1.HasPrevious);

        Assert.Equal(10, page2.Items.Count);
        Assert.True(page2.HasPrevious);
        Assert.True(page2.HasNext);

        Assert.Equal(5, page3.Items.Count);
        Assert.False(page3.HasNext);
    }

    [Fact]
    public async Task UserService_GetPaged_RespectsPageSize()
    {
        using var db = TestSupport.NewDb();
        var users = TestSupport.Users(db);

        for (int i = 1; i <= 15; i++)
        {
            await users.CreateAsync(new User
            {
                FullName = $"User {i}",
                Email = $"user{i}@assetops.com",
                Role = Roles.Employee,
                Department = "Engineering"
            }, "Pass@123", null);
        }

        var result = await users.GetPagedAsync(null, null, null, page: 1, pageSize: 5);
        Assert.Equal(15, result.TotalCount);
        Assert.Equal(5, result.Items.Count);
        Assert.Equal(3, result.TotalPages);
    }

    [Fact]
    public async Task RequestService_GetForApproval_FiltersByPriority()
    {
        using var db = TestSupport.NewDb();
        var category = await TestSupport.AddCategoryAsync(db);
        var employee = await TestSupport.AddUserAsync(db, "e@assetops.com", "Pass@123", Roles.Employee);
        var requests = TestSupport.Requests(db);

        await requests.CreateAsync(employee.Id, category.Id, "Need laptop for project work.", RequestPriority.High);
        await requests.CreateAsync(employee.Id, category.Id, "Need monitor for project work.", RequestPriority.Low);

        var highOnly = await requests.GetForApprovalAsync(
            RequestStatus.Pending, null, RequestPriority.High, null, null, 1, 10);

        Assert.Equal(1, highOnly.TotalCount);
        Assert.All(highOnly.Items, r => Assert.Equal(RequestPriority.High, r.Priority));
    }
}
