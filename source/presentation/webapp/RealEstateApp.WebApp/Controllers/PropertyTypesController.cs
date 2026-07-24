using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Dtos.Catalog;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Catalog;

namespace RealEstateApp.WebApp.Controllers;

[Authorize(Roles = "Admin")]
public class PropertyTypesController : Controller
{
    private readonly IPropertyTypeService _service;

    public PropertyTypesController(IPropertyTypeService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var dtos = await _service.GetAllAsync();

        if (dtos.Count == 0)
        {
            ViewBag.NoResultsMessage = "There are no property types registered at this time.";
        }

        var items = dtos.Select(d => new PropertyTypeViewModel
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
        return View(new PropertyTypeViewModel());
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PropertyTypeViewModel vm)
    {
        if (await _service.ExistsByNameAsync(vm.Name))
        {
            ModelState.AddModelError(nameof(vm.Name), "A property type with this name already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        var dto = new PropertyTypeDto { Name = vm.Name, Description = vm.Description };
        await _service.CreateAsync(dto);

        TempData["SuccessMessage"] = "The property type was created successfully.";
        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null)
        {
            TempData["ErrorMessage"] = "The requested property type does not exist.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new PropertyTypeViewModel
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
    public async Task<IActionResult> Edit(int id, PropertyTypeViewModel vm)
    {
        if (id != vm.Id)
        {
            TempData["ErrorMessage"] = "The requested property type does not exist.";
            return RedirectToAction(nameof(Index));
        }

        if (await _service.ExistsByNameAsync(vm.Name, vm.Id))
        {
            ModelState.AddModelError(nameof(vm.Name), "A property type with this name already exists.");
        }

        if (!ModelState.IsValid)
        {
            return View(vm);
        }

        try
        {
            var dto = new PropertyTypeDto { Id = vm.Id, Name = vm.Name, Description = vm.Description };
            await _service.UpdateAsync(id, dto);
            TempData["SuccessMessage"] = "The property type was updated successfully.";
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
            TempData["ErrorMessage"] = "The requested property type does not exist.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new PropertyTypeViewModel
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
            TempData["SuccessMessage"] = "The property type was deleted successfully.";
        }
        catch (DbUpdateException)
        {
            TempData["ErrorMessage"] = "This property type cannot be deleted because it has properties associated with it. Reassign or remove those properties first.";
        }

        return RedirectToAction(nameof(Index));
    }
}