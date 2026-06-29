using EnterpriseAssetManager.Models;
using EnterpriseAssetManager.Services;
using EnterpriseAssetManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAssetManager.Controllers;

[Authorize(Roles = "Manager,Admin")]
public class ApprovalsController : Controller
{
    private const int PageSize = 10;
    private readonly IRequestService _requests;
    private readonly IAssetService _assets;

    public ApprovalsController(IRequestService requests, IAssetService assets)
    {
        _requests = requests;
        _assets = assets;
    }

    public async Task<IActionResult> Index(
        RequestStatus? status, int? categoryId, RequestPriority? priority, DateTime? fromDate, DateTime? toDate, int page = 1)
    {
        var model = new ApprovalListViewModel
        {
            Status = status,
            CategoryId = categoryId,
            Priority = priority,
            FromDate = fromDate,
            ToDate = toDate,
            Categories = await _assets.GetAllCategoriesAsync(),
            Results = await _requests.GetForApprovalAsync(status, categoryId, priority, fromDate, toDate, page, PageSize)
        };
        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var request = await _requests.GetByIdAsync(id);
        if (request == null)
            return NotFound();

        return View(new RequestApprovalViewModel { RequestId = request.Id, Request = request });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Approve(int id, string comments)
    {
        try
        {
            await _requests.ApproveAsync(id, User.GetUserId(), comments);
            TempData["Success"] = "Request approved.";
        }
        catch (InvalidOperationException ex)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Reject(int id, string comments)
    {
        if (string.IsNullOrWhiteSpace(comments))
        {
            TempData["Error"] = "A comment is required when rejecting a request.";
            return RedirectToAction(nameof(Details), new { id });
        }

        try
        {
            await _requests.RejectAsync(id, User.GetUserId(), comments);
            TempData["Success"] = "Request rejected.";
        }
        catch (Exception ex) when (ex is InvalidOperationException or ArgumentException)
        {
            TempData["Error"] = ex.Message;
        }

        return RedirectToAction(nameof(Details), new { id });
    }

    // ---------- Admin only: fulfil an approved request ----------

    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Fulfill(int id)
    {
        var request = await _requests.GetByIdAsync(id);
        if (request == null)
            return NotFound();

        if (request.Status != RequestStatus.Approved)
        {
            TempData["Error"] = "Only approved requests can be fulfilled.";
            return RedirectToAction(nameof(Details), new { id });
        }

        var model = new FulfillRequestViewModel
        {
            RequestId = request.Id,
            Request = request,
            AvailableAssets = await _assets.GetAvailableByCategoryAsync(request.AssetCategoryId)
        };
        return View(model);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Fulfill(FulfillRequestViewModel model)
    {
        var request = await _requests.GetByIdAsync(model.RequestId);
        if (request == null)
            return NotFound();

        if (!ModelState.IsValid)
        {
            model.Request = request;
            model.AvailableAssets = await _assets.GetAvailableByCategoryAsync(request.AssetCategoryId);
            return View(model);
        }

        try
        {
            await _requests.FulfillAsync(model.RequestId, model.AssetId, User.GetUserId());
            TempData["Success"] = "Request fulfilled and asset assigned.";
            return RedirectToAction(nameof(Details), new { id = model.RequestId });
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            model.Request = request;
            model.AvailableAssets = await _assets.GetAvailableByCategoryAsync(request.AssetCategoryId);
            return View(model);
        }
    }
}
