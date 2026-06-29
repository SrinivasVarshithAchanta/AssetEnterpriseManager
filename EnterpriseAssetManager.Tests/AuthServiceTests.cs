using EnterpriseAssetManager.Models;
using Xunit;

namespace EnterpriseAssetManager.Tests;

public class AuthServiceTests
{
    [Fact]
    public async Task ValidateCredentials_ReturnsUser_OnCorrectLogin()
    {
        using var db = TestSupport.NewDb();
        await TestSupport.AddUserAsync(db, "admin@assetops.com", "Admin@123", Roles.Admin);
        var auth = TestSupport.Auth(db);

        var user = await auth.ValidateCredentialsAsync("admin@assetops.com", "Admin@123");

        Assert.NotNull(user);
        Assert.Equal(Roles.Admin, user.Role);
    }

    [Fact]
    public async Task ValidateCredentials_ReturnsNull_OnWrongPassword()
    {
        using var db = TestSupport.NewDb();
        await TestSupport.AddUserAsync(db, "admin@assetops.com", "Admin@123", Roles.Admin);
        var auth = TestSupport.Auth(db);

        var user = await auth.ValidateCredentialsAsync("admin@assetops.com", "WrongPass");

        Assert.Null(user);
    }

    [Fact]
    public async Task ValidateCredentials_ReturnsNull_ForInactiveUser()
    {
        using var db = TestSupport.NewDb();
        await TestSupport.AddUserAsync(db, "ex@assetops.com", "Pass@123", Roles.Employee, isActive: false);
        var auth = TestSupport.Auth(db);

        var user = await auth.ValidateCredentialsAsync("ex@assetops.com", "Pass@123");

        Assert.Null(user);
    }

    [Fact]
    public async Task BuildPrincipal_AddsRoleClaim()
    {
        using var db = TestSupport.NewDb();
        var seeded = await TestSupport.AddUserAsync(db, "m@assetops.com", "Pass@123", Roles.Manager);
        var auth = TestSupport.Auth(db);

        var principal = auth.BuildPrincipal(seeded);

        Assert.True(principal.IsInRole(Roles.Manager));
    }
}
