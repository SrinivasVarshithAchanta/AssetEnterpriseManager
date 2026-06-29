using EnterpriseAssetManager.Models;
using EnterpriseAssetManager.Services;
using Xunit;

namespace EnterpriseAssetManager.Tests;

public class UserServiceTests
{
    [Fact]
    public async Task Create_HashesPassword_NotPlainText()
    {
        using var db = TestSupport.NewDb();
        var users = TestSupport.Users(db);

        var created = await users.CreateAsync(new User
        {
            FullName = "New Person",
            Email = "new@assetops.com",
            Role = Roles.Employee,
            Department = "HR"
        }, "Plain@123", null);

        Assert.NotEqual("Plain@123", created.PasswordHash);
        Assert.True(new PasswordHasher().Verify("Plain@123", created.PasswordHash));
    }

    [Fact]
    public async Task Create_Throws_OnDuplicateEmail()
    {
        using var db = TestSupport.NewDb();
        await TestSupport.AddUserAsync(db, "dup@assetops.com", "Pass@123", Roles.Employee);
        var users = TestSupport.Users(db);

        await Assert.ThrowsAsync<InvalidOperationException>(() => users.CreateAsync(new User
        {
            FullName = "Dup",
            Email = "dup@assetops.com",
            Role = Roles.Employee
        }, "Pass@123", null));
    }

    [Fact]
    public async Task EmailExists_DetectsExistingEmail()
    {
        using var db = TestSupport.NewDb();
        await TestSupport.AddUserAsync(db, "exists@assetops.com", "Pass@123", Roles.Employee);
        var users = TestSupport.Users(db);

        Assert.True(await users.EmailExistsAsync("exists@assetops.com"));
        Assert.False(await users.EmailExistsAsync("missing@assetops.com"));
    }

    [Fact]
    public async Task GetPaged_FiltersByRole()
    {
        using var db = TestSupport.NewDb();
        await TestSupport.AddUserAsync(db, "a@assetops.com", "Pass@123", Roles.Admin);
        await TestSupport.AddUserAsync(db, "m@assetops.com", "Pass@123", Roles.Manager);
        await TestSupport.AddUserAsync(db, "e@assetops.com", "Pass@123", Roles.Employee);
        var users = TestSupport.Users(db);

        var managers = await users.GetPagedAsync(null, Roles.Manager, null, 1, 10);

        Assert.Equal(1, managers.TotalCount);
    }
}
