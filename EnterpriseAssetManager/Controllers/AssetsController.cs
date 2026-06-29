using EnterpriseAssetManager.Models;
using EnterpriseAssetManager.Services;
using EnterpriseAssetManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAssetManager.Controllers;

[Authorize(Roles = "Admin,Manager")]
public class AssetsController : Controller
{
    private const int PageSize = 10;
    private readonly IAssetService _assets;

    public AssetsController(IAssetService assets)
    {
        _assets = assets;
    }

    public async Task<IActionResult> Index(string search, int? categoryId, AssetStatus? status, AssetCondition? condition, int page = 1)
    {
        var model = new AssetListViewModel
        {
            Search = search,
            CategoryId = categoryId,
            Status = status,
            Condition = condition,
            Categories = await _assets.GetAllCategoriesAsync(),
            Results = await _assets.GetPagedAsync(search, categoryId, status, condition, page, PageSize)
        };

        return View(model);
    }

    public async Task<IActionResult> Details(int id)
    {
        var asset = await _assets.GetByIdAsync(id);
        if (asset == null)
            return NotFound();

        return View(asset);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Create()
    {
        var model = new AssetCreateViewModel
        {
            Categories = await _assets.GetActiveCategoriesAsync()
        };
        return View(model);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(AssetCreateViewModel model)
    {
        if (await _assets.AssetTagExistsAsync(model.AssetTag))
            ModelState.AddModelError(nameof(model.AssetTag), "This asset tag is already in use.");

        if (!ModelState.IsValid)
        {
            model.Categories = await _assets.GetActiveCategoriesAsync();
            return View(model);
        }

        var asset = new Asset
        {
            AssetTag = model.AssetTag,
            Name = model.Name,
            CategoryId = model.CategoryId,
            SerialNumber = model.SerialNumber,
            Status = model.Status,
            Condition = model.Condition,
            PurchaseDate = model.PurchaseDate
        };

        await _assets.CreateAsync(asset, User.GetUserId());
        TempData["Success"] = $"Asset {asset.AssetTag} created.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var asset = await _assets.GetByIdAsync(id);
        if (asset == null)
            return NotFound();

        var model = new AssetEditViewModel
        {
            Id = asset.Id,
            AssetTag = asset.AssetTag,
            Name = asset.Name,
            CategoryId = asset.CategoryId,
            SerialNumber = asset.SerialNumber,
            Status = asset.Status,
            Condition = asset.Condition,
            PurchaseDate = asset.PurchaseDate,
            Categories = await _assets.GetActiveCategoriesAsync()
        };

        return View(model);
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(AssetEditViewModel model)
    {
        if (await _assets.AssetTagExistsAsync(model.AssetTag, model.Id))
            ModelState.AddModelError(nameof(model.AssetTag), "This asset tag is already in use.");

        if (!ModelState.IsValid)
        {
            model.Categories = await _assets.GetActiveCategoriesAsync();
            return View(model);
        }

        var asset = new Asset
        {
            Id = model.Id,
            AssetTag = model.AssetTag,
            Name = model.Name,
            CategoryId = model.CategoryId,
            SerialNumber = model.SerialNumber,
            Status = model.Status,
            Condition = model.Condition,
            PurchaseDate = model.PurchaseDate
        };

        var updated = await _assets.UpdateAsync(asset, User.GetUserId());
        if (!updated)
            return NotFound();

        TempData["Success"] = $"Asset {asset.AssetTag} updated.";
        return RedirectToAction(nameof(Index));
    }

    [Authorize(Roles = Roles.Admin)]
    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Retire(int id)
    {
        var retired = await _assets.RetireAsync(id, User.GetUserId());
        TempData[retired ? "Success" : "Error"] = retired ? "Asset retired." : "Asset not found.";
        return RedirectToAction(nameof(Index));
    }
}
