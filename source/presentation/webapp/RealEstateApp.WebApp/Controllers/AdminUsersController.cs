using System.Security.Claims;
using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Dtos.Account;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Account;

namespace RealEstateApp.WebApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminUsersController : Controller
{
    private readonly IAdminUserService _adminUserService;
    private readonly IMapper _mapper;

    public AdminUsersController(IAdminUserService adminUserService, IMapper mapper)
    {
        _adminUserService = adminUserService;
        _mapper = mapper;
    }

    private string CurrentUserId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    public async Task<IActionResult> Index()
    {
        var admins = await _adminUserService.GetAllAsync();

        if (admins.Count == 0)
        {
            ViewBag.NoResultsMessage = "There are no administrators registered at this time.";
        }

        ViewBag.CurrentUserId = CurrentUserId;
        return View(_mapper.Map<List<AdminUserViewModel>>(admins));
    }

    public IActionResult Create() => View(new SaveAdminUserViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SaveAdminUserViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _adminUserService.CreateAsync(_mapper.Map<SaveAdminUserDto>(vm));
            TempData["SuccessMessage"] = "The administrator was created successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }

    public async Task<IActionResult> Edit(string id)
    {
        if (id == CurrentUserId)
        {
            TempData["ErrorMessage"] = "You cannot edit your own administrator account.";
            return RedirectToAction(nameof(Index));
        }

        var admin = await _adminUserService.GetByIdAsync(id);
        if (admin is null)
        {
            TempData["ErrorMessage"] = "The requested administrator does not exist.";
            return RedirectToAction(nameof(Index));
        }

        return View(_mapper.Map<SaveAdminUserViewModel>(admin));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SaveAdminUserViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _adminUserService.UpdateAsync(_mapper.Map<SaveAdminUserDto>(vm), CurrentUserId);
            TempData["SuccessMessage"] = "The administrator was updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
        catch (InvalidOperationException ex)
        {
            ModelState.AddModelError(string.Empty, ex.Message);
            return View(vm);
        }
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(string id, bool isActive)
    {
        try
        {
            await _adminUserService.ChangeStatusAsync(id, isActive, CurrentUserId);
            TempData["SuccessMessage"] = isActive
                ? "The administrator was activated successfully."
                : "The administrator was deactivated successfully.";
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }
        catch (InvalidOperationException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}