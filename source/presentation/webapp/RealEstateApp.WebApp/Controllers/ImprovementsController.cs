using AutoMapper;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Dtos.Catalog;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Catalog;

namespace RealEstateApp.WebApp.Controllers;

[Authorize(Roles = "Admin")]
public class ImprovementsController : Controller
{
    private readonly IImprovementService _service;
    private readonly IMapper _mapper;

    public ImprovementsController(IImprovementService service, IMapper mapper)
    {
        _service = service;
        _mapper = mapper;
    }

    public async Task<IActionResult> Index()
    {
        var list = await _service.GetAllAsync();

        if (list.Count == 0)
        {
            ViewBag.NoResultsMessage = "There are no improvements registered at this time.";
        }

        return View(_mapper.Map<List<ImprovementViewModel>>(list));
    }

    public IActionResult Create() => View(new ImprovementViewModel());

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Create(ImprovementViewModel vm)
    {
        if (await _service.ExistsByNameAsync(vm.Name))
        {
            ModelState.AddModelError(nameof(vm.Name), "An improvement with this name already exists.");
        }

        if (!ModelState.IsValid) return View(vm);

        await _service.CreateAsync(_mapper.Map<ImprovementDto>(vm));
        TempData["SuccessMessage"] = "The improvement was created successfully.";
        return RedirectToAction(nameof(Index));
    }

    public async Task<IActionResult> Edit(int id)
    {
        var dto = await _service.GetByIdAsync(id);
        if (dto is null)
        {
            TempData["ErrorMessage"] = "The requested improvement does not exist.";
            return RedirectToAction(nameof(Index));
        }

        return View(_mapper.Map<ImprovementViewModel>(dto));
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> Edit(ImprovementViewModel vm)
    {
        if (await _service.ExistsByNameAsync(vm.Name, vm.Id))
        {
            ModelState.AddModelError(nameof(vm.Name), "An improvement with this name already exists.");
        }

        if (!ModelState.IsValid) return View(vm);

        try
        {
            await _service.UpdateAsync(vm.Id, _mapper.Map<ImprovementDto>(vm));
            TempData["SuccessMessage"] = "The improvement was updated successfully.";
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
            TempData["ErrorMessage"] = "The requested improvement does not exist.";
            return RedirectToAction(nameof(Index));
        }

        return View(_mapper.Map<ImprovementViewModel>(dto));
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(int id)
    {
        try
        {
            await _service.DeleteAsync(id);
            TempData["SuccessMessage"] = "The improvement was deleted successfully. It has been removed from any properties that had it.";
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }
}