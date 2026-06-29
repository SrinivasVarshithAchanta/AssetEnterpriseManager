using EnterpriseAssetManager.Models;
using EnterpriseAssetManager.Services;
using EnterpriseAssetManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAssetManager.Controllers;

// Any signed in user can raise and track their own requests.
[Authorize]
public class RequestsController : Controller
{
    private const int PageSize = 10;
    private readonly IRequestService _requests;
    private readonly IAssetService _assets;

    public RequestsController(IRequestService requests, IAssetService assets)
    {
        _requests = requests;
        _assets = assets;
    }

    public async Task<IActionResult> Index(RequestStatus? status, int page = 1)
    {
        var model = new MyRequestListViewModel
        {
            Status = status,
            Results = await _requests.GetMyRequestsAsync(User.GetUserId(), status, page, PageSize)
        };
        return View(model);
    }

    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new AssetRequestCreateViewModel
        {
            Categories = await _assets.GetActiveCategoriesAsync()
        };
        return View(model);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssetRequestCreateViewModel model)
    {
        if (!ModelState.IsValid)
        {
            model.Categories = await _assets.GetActiveCategoriesAsync();
            return View(model);
        }

        var request = await _requests.CreateAsync(
            User.GetUserId(), model.AssetCategoryId, model.BusinessReason, model.Priority);

        TempData["Success"] = $"Request {request.RequestNumber} submitted.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Details(int id)
    {
        var request = await _requests.GetByIdAsync(id);
        if (request == null)
            return NotFound();

        // Employees may only view their own requests. Managers/Admins can view any.
        var role = User.GetRole();
        if (role == Roles.Employee && request.RequestedByUserId != User.GetUserId())
            return RedirectToAction("AccessDenied", "Account");

        return View(request);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Cancel(int id)
    {
        try
        {
            var cancelled = await _requests.CancelAsync(id, User.GetUserId());
            TempData[cancelled ? "Success" : "Error"] = cancelled ? "Request cancelled." : "Request not found.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or UnauthorizedAccessException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}
