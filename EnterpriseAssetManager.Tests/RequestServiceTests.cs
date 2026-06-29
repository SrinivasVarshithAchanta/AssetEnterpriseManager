using EnterpriseAssetManager.Data;
using EnterpriseAssetManager.Models;
using EnterpriseAssetManager.Services;
using Xunit;

namespace EnterpriseAssetManager.Tests;

public class RequestServiceTests
{
    [Fact]
    public async Task Employee_CreateRequest_SetsPendingStatus()
    {
        using var db = TestSupport.NewDb();
        var category = await TestSupport.AddCategoryAsync(db);
        var employee = await TestSupport.AddUserAsync(db, "e@assetops.com", "Pass@123", Roles.Employee);
        var requests = TestSupport.Requests(db);

        var request = await requests.CreateAsync(
            employee.Id, category.Id, "Need a laptop for daily development work.", RequestPriority.Medium);

        Assert.Equal(RequestStatus.Pending, request.Status);
        Assert.Equal(employee.Id, request.RequestedByUserId);
        Assert.False(string.IsNullOrWhiteSpace(request.RequestNumber));
    }

    [Fact]
    public async Task GenerateRequestNumber_ProducesUniqueValues()
    {
        using var db = TestSupport.NewDb();
        var category = await TestSupport.AddCategoryAsync(db);
        var employee = await TestSupport.AddUserAsync(db, "e@assetops.com", "Pass@123", Roles.Employee);
        var requests = TestSupport.Requests(db);

        var first = await requests.CreateAsync(employee.Id, category.Id, "Need a laptop for work.", RequestPriority.Medium);
        var second = await requests.CreateAsync(employee.Id, category.Id, "Need a monitor for work.", RequestPriority.Low);

        Assert.NotEqual(first.RequestNumber, second.RequestNumber);
    }

    [Fact]
    public async Task Reject_WithoutComment_Throws()
    {
        using var db = TestSupport.NewDb();
        var (requests, request, managerId) = await SeedPendingRequestAsync(db);

        await Assert.ThrowsAsync<ArgumentException>(() => requests.RejectAsync(request.Id, managerId, "   "));
    }

    [Fact]
    public async Task Reject_WithComment_SetsRejected()
    {
        using var db = TestSupport.NewDb();
        var (requests, request, managerId) = await SeedPendingRequestAsync(db);

        var ok = await requests.RejectAsync(request.Id, managerId, "Out of budget this quarter.");

        Assert.True(ok);
        var reloaded = await requests.GetByIdAsync(request.Id);
        Assert.Equal(RequestStatus.Rejected, reloaded.Status);
        Assert.Equal("Out of budget this quarter.", reloaded.ManagerComments);
    }

    [Fact]
    public async Task Approve_SetsApprovedAndReviewer()
    {
        using var db = TestSupport.NewDb();
        var (requests, request, managerId) = await SeedPendingRequestAsync(db);

        var ok = await requests.ApproveAsync(request.Id, managerId, "Looks fine.");

        Assert.True(ok);
        var reloaded = await requests.GetByIdAsync(request.Id);
        Assert.Equal(RequestStatus.Approved, reloaded.Status);
        Assert.Equal(managerId, reloaded.ReviewedByUserId);
    }

    [Fact]
    public async Task Fulfill_WithRetiredAsset_Throws()
    {
        using var db = TestSupport.NewDb();
        var (requests, request, managerId) = await SeedPendingRequestAsync(db);
        await requests.ApproveAsync(request.Id, managerId, null);

        var retired = await TestSupport.AddAssetAsync(db, request.AssetCategoryId, "AST-RET", AssetStatus.Retired);
        var admin = await TestSupport.AddUserAsync(db, "admin@assetops.com", "Admin@123", Roles.Admin);

        await Assert.ThrowsAsync<InvalidOperationException>(() => requests.FulfillAsync(request.Id, retired.Id, admin.Id));
    }

    [Fact]
    public async Task Fulfill_AssignsAsset_AndSetsStatuses()
    {
        using var db = TestSupport.NewDb();
        var (requests, request, managerId) = await SeedPendingRequestAsync(db);
        await requests.ApproveAsync(request.Id, managerId, null);

        var asset = await TestSupport.AddAssetAsync(db, request.AssetCategoryId, "AST-OK", AssetStatus.Available);
        var admin = await TestSupport.AddUserAsync(db, "admin@assetops.com", "Admin@123", Roles.Admin);

        var ok = await requests.FulfillAsync(request.Id, asset.Id, admin.Id);

        Assert.True(ok);

        var assets = TestSupport.Assets(db);
        var reloadedAsset = await assets.GetByIdAsync(asset.Id);
        Assert.Equal(AssetStatus.Assigned, reloadedAsset.Status);
        Assert.Equal(request.RequestedByUserId, reloadedAsset.AssignedToUserId);

        var reloadedRequest = await requests.GetByIdAsync(request.Id);
        Assert.Equal(RequestStatus.Fulfilled, reloadedRequest.Status);
        Assert.Equal(asset.Id, reloadedRequest.AssetId);
    }

    [Fact]
    public async Task Fulfill_NonApprovedRequest_Throws()
    {
        using var db = TestSupport.NewDb();
        var (requests, request, _) = await SeedPendingRequestAsync(db);
        var asset = await TestSupport.AddAssetAsync(db, request.AssetCategoryId, "AST-PEND", AssetStatus.Available);

        await Assert.ThrowsAsync<InvalidOperationException>(() => requests.FulfillAsync(request.Id, asset.Id, 1));
    }

    [Fact]
    public async Task Cancel_ByNonOwner_Throws()
    {
        using var db = TestSupport.NewDb();
        var (requests, request, _) = await SeedPendingRequestAsync(db);

        await Assert.ThrowsAsync<UnauthorizedAccessException>(() => requests.CancelAsync(request.Id, 999));
    }

    [Fact]
    public async Task Cancel_ByOwner_SetsCancelled()
    {
        using var db = TestSupport.NewDb();
        var (requests, request, _) = await SeedPendingRequestAsync(db);

        var ok = await requests.CancelAsync(request.Id, request.RequestedByUserId);

        Assert.True(ok);
        var reloaded = await requests.GetByIdAsync(request.Id);
        Assert.Equal(RequestStatus.Cancelled, reloaded.Status);
    }

    // Creates a category, an employee, a manager and a single pending request.
    private static async Task<(RequestService requests, AssetRequest request, int managerId)>
        SeedPendingRequestAsync(ApplicationDbContext db)
    {
        var category = await TestSupport.AddCategoryAsync(db);
        var employee = await TestSupport.AddUserAsync(db, "e@assetops.com", "Pass@123", Roles.Employee);
        var manager = await TestSupport.AddUserAsync(db, "m@assetops.com", "Pass@123", Roles.Manager);
        var requests = TestSupport.Requests(db);

        var request = await requests.CreateAsync(employee.Id, category.Id, "Need a laptop for development.", RequestPriority.High);
        return (requests, request, manager.Id);
    }
}
