using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Dtos.Catalog;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Catalog;

namespace RealEstateApp.WebApp.Controllers;

[Authorize(Roles = "Admin")]
public class ImprovementsController : Controller
{
    private readonly IImprovementService _service;

    public ImprovementsController(IImprovementService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var dtos = await _service.GetAllAsync();

        if (dtos.Count == 0)
        {
            ViewBag.NoResultsMessage = "There are no improvements registered at this time.";
        }

        var items = dtos.Select(d => new ImprovementViewModel
        {
            Id = d.Id,
            Name = d.Name,
            Description = d.Description,
            PropertiesCount = d.PropertiesCount
        }).ToList();

        return View(items);
    }

    [HttpGet]
    public IActionResult Create()
    {
        return View(new ImprovementViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ImprovementViewModel vm)
    {
        if (await _service.ExistsByNameAsync(vm.Name))
        {
            ModelState.AddModelError(nameof(vm.Name), "An improvement with this name already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var dto = new ImprovementDto { Name = vm.Name, Description = vm.Description };
        await _service.CreateAsync(dto);

        TempData["SuccessMessage"] = "The improvement was created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null)
        {
            TempData["ErrorMessage"] = "The requested improvement does not exist.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new ImprovementViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            PropertiesCount = dto.PropertiesCount
        };

        return View(vm);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(int id, ImprovementViewModel vm)
    {
        if (id != vm.Id)
        {
            TempData["ErrorMessage"] = "The requested improvement does not exist.";
            return RedirectToAction(nameof(Index));
        }

        if (await _service.ExistsByNameAsync(vm.Name, vm.Id))
        {
            ModelState.AddModelError(nameof(vm.Name), "An improvement with this name already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        try
        {
            var dto = new ImprovementDto { Id = vm.Id, Name = vm.Name, Description = vm.Description };
            await _service.UpdateAsync(id, dto);
            TempData["SuccessMessage"] = "The improvement was updated successfully.";
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null)
        {
            TempData["ErrorMessage"] = "The requested improvement does not exist.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new ImprovementViewModel
        {
            Id = dto.Id,
            Name = dto.Name,
            Description = dto.Description,
            PropertiesCount = dto.PropertiesCount
        };

        return View(vm);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            TempData["SuccessMessage"] = "The improvement was deleted successfully. It was also removed from any properties that had it, without affecting those properties otherwise.";
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "The improvement could not be deleted due to a data conflict. Please try again.";
        }

        return RedirectToAction(nameof(Index));
    }
}