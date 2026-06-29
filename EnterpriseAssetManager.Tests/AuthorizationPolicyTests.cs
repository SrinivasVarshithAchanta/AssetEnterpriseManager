using System.Reflection;
using EnterpriseAssetManager.Controllers;
using EnterpriseAssetManager.Models;
using Microsoft.AspNetCore.Authorization;
using Xunit;

namespace EnterpriseAssetManager.Tests;

// Authorization is enforced with [Authorize(Roles = ...)] attributes. These tests
// confirm the policy on the controllers so role rules cannot be silently removed.
// In particular, an Employee must not be able to reach the approval actions.
public class AuthorizationPolicyTests
{
    private static List<string> RolesOf(AuthorizeAttribute attribute) =>
        (attribute?.Roles ?? string.Empty)
            .Split(',', StringSplitOptions.RemoveEmptyEntries)
            .Select(r => r.Trim())
            .ToList();

    [Fact]
    public void Approvals_RequireManagerOrAdmin_AndExcludeEmployee()
    {
        var attribute = typeof(ApprovalsController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attribute);

        var roles = RolesOf(attribute);
        Assert.Contains(Roles.Manager, roles);
        Assert.Contains(Roles.Admin, roles);
        Assert.DoesNotContain(Roles.Employee, roles);
    }

    [Fact]
    public void FulfillActions_AreAdminOnly()
    {
        var fulfillMethods = typeof(ApprovalsController)
            .GetMethods()
            .Where(m => m.Name == "Fulfill")
            .ToList();

        Assert.NotEmpty(fulfillMethods);
        Assert.All(fulfillMethods, m =>
        {
            var attribute = m.GetCustomAttribute<AuthorizeAttribute>();
            Assert.NotNull(attribute);
            Assert.Equal(Roles.Admin, attribute.Roles);
        });
    }

    [Fact]
    public void UserManagement_IsAdminOnly()
    {
        var attribute = typeof(UsersController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal(Roles.Admin, attribute.Roles);
    }

    [Fact]
    public void AuditLogs_AreAdminOnly()
    {
        var attribute = typeof(AuditLogsController).GetCustomAttribute<AuthorizeAttribute>();
        Assert.NotNull(attribute);
        Assert.Equal(Roles.Admin, attribute.Roles);
    }
}
