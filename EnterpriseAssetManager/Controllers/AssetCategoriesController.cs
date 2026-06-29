using EnterpriseAssetManager.Models;
using EnterpriseAssetManager.Services;
using EnterpriseAssetManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAssetManager.Controllers;

[Authorize(Roles = Roles.Admin)]
public class AssetCategoriesController : Controller
{
    private readonly IAssetService _assets;

    public AssetCategoriesController(IAssetService assets)
    {
        _assets = assets;
    }

    public async Task<IActionResult> Index()
    {
        var model = new CategoryListViewModel
        {
            Categories = await _assets.GetAllCategoriesAsync()
        };
        return View(model);
    }

    [HttpGet]
    public IActionResult Create() => View(new CategoryFormViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(CategoryFormViewModel model)
    {
        if (await _assets.CategoryNameExistsAsync(model.Name))
            ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");

        if (!ModelState.IsValid)
            return View(model);

        await _assets.CreateCategoryAsync(new AssetCategory
        {
            Name = model.Name,
            Description = model.Description,
            IsActive = model.IsActive
        }, User.GetUserId());

        TempData["Success"] = $"Category {model.Name} created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var category = await _assets.GetCategoryByIdAsync(id);
        if (category == null)
            return NotFound();

        return View(new CategoryFormViewModel
        {
            Id = category.Id,
            Name = category.Name,
            Description = category.Description,
            IsActive = category.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(CategoryFormViewModel model)
    {
        if (await _assets.CategoryNameExistsAsync(model.Name, model.Id))
            ModelState.AddModelError(nameof(model.Name), "A category with this name already exists.");

        if (!ModelState.IsValid)
            return View(model);

        var updated = await _assets.UpdateCategoryAsync(new AssetCategory
        {
            Id = model.Id,
            Name = model.Name,
            Description = model.Description,
            IsActive = model.IsActive
        }, User.GetUserId());

        if (!updated)
            return NotFound();

        TempData["Success"] = $"Category {model.Name} updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(int id, bool isActive)
    {
        var changed = await _assets.SetCategoryActiveAsync(id, isActive, User.GetUserId());
        TempData[changed ? "Success" : "Error"] = changed ? "Category updated." : "Category not found.";
        return RedirectToAction(nameof(Index));
    }
}
