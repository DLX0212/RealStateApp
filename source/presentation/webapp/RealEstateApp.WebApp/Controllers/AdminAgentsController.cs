using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Application.ViewModels.Agent;

namespace RealEstateApp.WebApp.Controllers;

[Authorize(Roles = "Admin")]
public class AdminAgentsController : Controller
{
    private readonly IAgentService _agentService;

    public AdminAgentsController(IAgentService agentService)
    {
        _agentService = agentService;
    }

    public async Task<IActionResult> Index()
    {
        var agents = await _agentService.GetAllAsync();

        if (agents.Count == 0)
        {
            ViewBag.NoResultsMessage = "There are no agents registered at this time.";
        }

        var items = agents.Select(a => new AgentAdminViewModel
        {
            Id = a.Id,
            FirstName = a.FirstName,
            LastName = a.LastName,
            Email = a.Email,
            PropertiesCount = a.PropertiesCount,
            IsActive = a.IsActive
        }).ToList();

        return View(items);
    }

    [HttpPost]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> ChangeStatus(string id, bool isActive)
    {
        try
        {
            await _agentService.ChangeStatusAsync(id, isActive);
            TempData["SuccessMessage"] = isActive
                ? "The agent was activated successfully."
                : "The agent was deactivated successfully.";
        }
        catch (KeyNotFoundException ex)
        {
            TempData["ErrorMessage"] = ex.Message;
        }

        return RedirectToAction(nameof(Index));
    }

    [HttpGet]
    public async Task<IActionResult> Delete(string id)
    {
        var agent = await _agentService.GetByIdAsync(id);
        if (agent is null)
        {
            TempData["ErrorMessage"] = "The requested agent does not exist.";
            return RedirectToAction(nameof(Index));
        }

        var vm = new AgentAdminViewModel
        {
            Id = agent.Id,
            FirstName = agent.FirstName,
            LastName = agent.LastName,
            Email = agent.Email,
            PropertiesCount = agent.PropertiesCount,
            IsActive = agent.IsActive
        };

        return View(vm);
    }

    [HttpPost, ActionName("Delete")]
    [ValidateAntiForgeryToken]
    public async Task<IActionResult> DeleteConfirmed(string id)
    {
        try
        {
            await _agentService.DeleteAsync(id);
            TempData["SuccessMessage"] = "The agent and their properties were deleted successfully.";
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