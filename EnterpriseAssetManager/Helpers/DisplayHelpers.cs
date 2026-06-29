using EnterpriseAssetManager.Models;

namespace EnterpriseAssetManager.Helpers;

// Tiny presentation helpers so views can render consistent status badge styles
// without embedding business logic in Razor.
public static class DisplayHelpers
{
    public static string BadgeClass(RequestStatus status) => status switch
    {
        RequestStatus.Pending => "badge badge-pending",
        RequestStatus.Approved => "badge badge-approved",
        RequestStatus.Rejected => "badge badge-rejected",
        RequestStatus.Fulfilled => "badge badge-fulfilled",
        RequestStatus.Cancelled => "badge badge-cancelled",
        _ => "badge"
    };

    public static string BadgeClass(AssetStatus status) => status switch
    {
        AssetStatus.Available => "badge badge-available",
        AssetStatus.Assigned => "badge badge-assigned",
        AssetStatus.UnderMaintenance => "badge badge-maintenance",
        AssetStatus.Retired => "badge badge-retired",
        _ => "badge"
    };

    public static string Friendly(AssetStatus status) =>
        status == AssetStatus.UnderMaintenance ? "Under Maintenance" : status.ToString();
}
