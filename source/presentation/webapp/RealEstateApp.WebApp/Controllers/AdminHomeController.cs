using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Dashboard;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.WebApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminHomeController : Controller
{
    private readonly IPropertyService _propertyService;
    private readonly IAccountService _accountService;

    public AdminHomeController(IPropertyService propertyService, IAccountService accountService)
    {
        _propertyService = propertyService;
        _accountService = accountService;
    }

    public async Task<IActionResult> Index()
    {
        var properties = await _propertyService.GetAllAsync();

        var vm = new AdminDashboardViewModel
        {
            AvailableProperties = properties.Count(p => p.Status == nameof(PropertyStatus.Available)),
            SoldProperties = properties.Count(p => p.Status == nameof(PropertyStatus.Sold)),

            ActiveAgents = await _accountService.CountActiveByRoleAsync(nameof(RoleType.Agent)),
            InactiveAgents = await _accountService.CountInactiveByRoleAsync(nameof(RoleType.Agent)),

            ActiveClients = await _accountService.CountActiveByRoleAsync(nameof(RoleType.Client)),
            InactiveClients = await _accountService.CountInactiveByRoleAsync(nameof(RoleType.Client)),

            ActiveDevelopers = await _accountService.CountActiveByRoleAsync(nameof(RoleType.Developer)),
            InactiveDevelopers = await _accountService.CountInactiveByRoleAsync(nameof(RoleType.Developer))
        };

        return View(vm);
    }
}