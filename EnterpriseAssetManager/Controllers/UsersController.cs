using EnterpriseAssetManager.Models;
using EnterpriseAssetManager.Services;
using EnterpriseAssetManager.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EnterpriseAssetManager.Controllers;

[Authorize(Roles = Roles.Admin)]
public class UsersController : Controller
{
    private const int PageSize = 10;
    private readonly IUserService _users;

    public UsersController(IUserService users)
    {
        _users = users;
    }

    public async Task<IActionResult> Index(string search, string role, bool? isActive, int page = 1)
    {
        var model = new UserListViewModel
        {
            Search = search,
            Role = role,
            IsActive = isActive,
            Results = await _users.GetPagedAsync(search, role, isActive, page, PageSize)
        };
        return View(model);
    }

    [HttpGet]
    public IActionResult Create() => View(new UserCreateViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(UserCreateViewModel model)
    {
        if (await _users.EmailExistsAsync(model.Email))
            ModelState.AddModelError(nameof(model.Email), "This email is already registered.");

        if (!ModelState.IsValid)
            return View(model);

        await _users.CreateAsync(new User
        {
            FullName = model.FullName,
            Email = model.Email,
            Role = model.Role,
            Department = model.Department,
            IsActive = model.IsActive
        }, model.Password, User.GetUserId());

        TempData["Success"] = $"User {model.Email} created.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var user = await _users.GetByIdAsync(id);
        if (user == null)
            return NotFound();

        return View(new UserEditViewModel
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role,
            Department = user.Department,
            IsActive = user.IsActive
        });
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(UserEditViewModel model)
    {
        if (!ModelState.IsValid)
            return View(model);

        var updated = await _users.UpdateAsync(new User
        {
            Id = model.Id,
            FullName = model.FullName,
            Role = model.Role,
            Department = model.Department,
            IsActive = model.IsActive
        }, User.GetUserId());

        if (!updated)
            return NotFound();

        TempData["Success"] = $"User {model.Email} updated.";
        return RedirectToAction(nameof(Index));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> SetActive(int id, bool isActive)
    {
        var changed = await _users.SetActiveAsync(id, isActive, User.GetUserId());
        TempData[changed ? "Success" : "Error"] = changed ? "User updated." : "User not found.";
        return RedirectToAction(nameof(Index));
    }
}
