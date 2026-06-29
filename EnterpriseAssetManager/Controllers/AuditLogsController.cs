using EnterpriseAssetManager.Models;
using EnterpriseAssetManager.Services;
using EnterpriseAssetManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAssetManager.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AuditLogsController : Controller
{
    private const int PageSize = 15;
    private readonly IAuditService _audit;

    public AuditLogsController(IAuditService audit)
    {
        _audit = audit;
    }

    public async Task<IActionResult> Index(string search, int page = 1)
    {
        var model = new AuditLogListViewModel
        {
            Search = search,
            Results = await _audit.GetPagedAsync(search, page, PageSize)
        };
        return View(model);
    }
}
