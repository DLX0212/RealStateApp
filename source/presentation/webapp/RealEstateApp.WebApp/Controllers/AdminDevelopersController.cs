using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Dtos.Account;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Account;

namespace RealEstateApp.WebApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminDevelopersController : Controller
{
    private readonly IDeveloperService _developerService;
    private readonly IMapper _mapper;

    public AdminDevelopersController(IDeveloperService developerService, IMapper mapper)
    {
        _developerService = developerService;
        _mapper = mapper;
    }

    public async Task<IActionResult> Index()
    {
        var developers = await _developerService.GetAllAsync();

        if (developers.Count == 0)
        {
            ViewBag.NoResultsMessage = "There are no developers registered at this time.";
        }

        return View(_mapper.Map<List<DeveloperUserViewModel>>(developers));
    }

    public IActionResult Create() => View(new SaveDeveloperUserViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(SaveDeveloperUserViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _developerService.CreateAsync(_mapper.Map<SaveDeveloperUserDto>(vm));
            TempData["SuccessMessage"] = "The developer was created successfully.";
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
        var developer = await _developerService.GetByIdAsync(id);
        if (developer is null)
        {
            TempData["ErrorMessage"] = "The requested developer does not exist.";
            return RedirectToAction(nameof(Index));
        }

        return View(_mapper.Map<SaveDeveloperUserViewModel>(developer));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(SaveDeveloperUserViewModel vm)
    {
        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _developerService.UpdateAsync(_mapper.Map<SaveDeveloperUserDto>(vm));
            TempData["SuccessMessage"] = "The developer was updated successfully.";
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
            await _developerService.ChangeStatusAsync(id, isActive);
            TempData["SuccessMessage"] = isActive
                ? "The developer was activated successfully."
                : "The developer was deactivated successfully.";
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}