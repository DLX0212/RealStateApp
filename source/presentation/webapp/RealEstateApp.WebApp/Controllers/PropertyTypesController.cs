using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Dtos.Catalog;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Catalog;

namespace RealEstateApp.WebApp.Controllers;

[Authorize(Roles = "Admin")]
public class PropertyTypesController : Controller
{
    private readonly IPropertyTypeService _service;
    private readonly IMapper _mapper;

    public PropertyTypesController(IPropertyTypeService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _service.GetAllAsync();

        if (list.Count == 0)
        {
            ViewBag.NoResultsMessage = "There are no property types registered at this time.";
        }

        return View(_mapper.Map<List<PropertyTypeViewModel>>(list));
    }

    public IActionResult Create() => View(new PropertyTypeViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(PropertyTypeViewModel vm)
    {
        if (await _service.ExistsByNameAsync(vm.Name))
        {
            ModelState.AddModelError(nameof(vm.Name), "A property type with this name already exists.");
        }

        if (!ModelState.IsValid) return View(vm);

        await _service.CreateAsync(_mapper.Map<PropertyTypeDto>(vm));
        TempData["SuccessMessage"] = "The property type was created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null)
        {
            TempData["ErrorMessage"] = "The requested property type does not exist.";
            return RedirectToAction(nameof(Index));
        }

        return View(_mapper.Map<PropertyTypeViewModel>(dto));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(PropertyTypeViewModel vm)
    {
        if (await _service.ExistsByNameAsync(vm.Name, vm.Id))
        {
            ModelState.AddModelError(nameof(vm.Name), "A property type with this name already exists.");
        }

        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _service.UpdateAsync(vm.Id, _mapper.Map<PropertyTypeDto>(vm));
            TempData["SuccessMessage"] = "The property type was updated successfully.";
            return RedirectToAction(nameof(Index));
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
            return RedirectToAction(nameof(Index));
        }
    }

    public async Task<IActionResult> Delete(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null)
        {
            TempData["ErrorMessage"] = "The requested property type does not exist.";
            return RedirectToAction(nameof(Index));
        }

        return View(_mapper.Map<PropertyTypeViewModel>(dto));
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