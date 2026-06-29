using EnterpriseAssetManager.Models;

namespace EnterpriseAssetManager.ViewModels;

public class DashboardViewModel
{
    public string Role { get; set; }
    public string UserName { get; set; }

    // For employees this holds their own request counts; for admins/managers it is global.
    public RequestStats RequestStats { get; set; } = new();

    // Only populated for admin and manager dashboards.
    public AssetStats AssetStats { get; set; }

    public List<AssetRequest> RecentRequests { get; set; } = new();
}
