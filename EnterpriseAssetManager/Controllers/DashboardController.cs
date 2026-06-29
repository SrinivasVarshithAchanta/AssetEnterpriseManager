using EnterpriseAssetManager.Models;
using EnterpriseAssetManager.Services;
using EnterpriseAssetManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAssetManager.Controllers;

[Authorize]
public class DashboardController : Controller
{
    private readonly IRequestService _requests;
    private readonly IAssetService _assets;

    public DashboardController(IRequestService requests, IAssetService assets)
    {
        _requests = requests;
        _assets = assets;
    }

    public async Task<IActionResult> Index()
    {
        var role = User.GetRole();
        var userId = User.GetUserId();

        var model = new DashboardViewModel
        {
            Role = role,
            UserName = User.GetDisplayName()
        };

        if (role == Roles.Employee)
        {
            // Employees see only their own request summary.
            model.RequestStats = await _requests.GetStatsForUserAsync(userId);
        }
        else
        {
            // Admins and managers see organization wide numbers and recent activity.
            model.RequestStats = await _requests.GetGlobalStatsAsync();
            model.AssetStats = await _assets.GetStatsAsync();
            model.RecentRequests = await _requests.GetRecentlyReviewedAsync(8);
        }

        return View(model);
    }
}
