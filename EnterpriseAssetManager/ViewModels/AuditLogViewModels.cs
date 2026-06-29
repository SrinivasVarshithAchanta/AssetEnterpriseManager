using EnterpriseAssetManager.Models;

namespace EnterpriseAssetManager.ViewModels;

public class AuditLogListViewModel
{
    public PagedResult<AuditLog> Results { get; set; } = new();
    public string Search { get; set; }
}
