using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class AgentsController : ControllerBase
{
    private readonly IAgentService _agentService;
    private readonly IPropertyService _propertyService;

    public AgentsController(IAgentService agentService, IPropertyService propertyService)
    {
        _agentService = agentService;
        _propertyService = propertyService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Developer")]
    public async Task<IActionResult> GetAll()
    {
        var agents = await _agentService.GetAllAsync();

        if (agents.Count == 0)
        {
            return NoContent();
        }

        return Ok(agents);
    }

    [HttpGet("search")]
    [Authorize(Roles = "Admin,Developer")]
    public async Task<IActionResult> Search([FromQuery] string name)
    {
        var agents = await _agentService.SearchActiveByNameAsync(name ?? string.Empty);

        if (agents.Count == 0)
        {
            return NoContent();
        }

        return Ok(agents);
    }
    [HttpGet("{id}")]
    [Authorize(Roles = "Admin,Developer")]
    public async Task<IActionResult> GetById(string id)
    {
        var agent = await _agentService.GetByIdAsync(id);

        if (agent is null)
        {
            return NotFound(new { message = "The requested agent does not exist." });
        }

        return Ok(agent);
    }

    [HttpGet("{id}/properties")]
    [Authorize(Roles = "Admin,Developer")]
    public async Task<IActionResult> GetAgentProperties(string id)
    {
        var agent = await _agentService.GetByIdAsync(id);
        if (agent is null)
        {
            return NotFound(new { message = "The requested agent does not exist." });
        }

        var properties = await _propertyService.GetByAgentIdAsync(id);

        if (properties.Count == 0)
        {
            return NoContent();
        }

        return Ok(properties);
    }

    [HttpPatch("{id}/status")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> ChangeStatus(string id, [FromBody] ChangeAgentStatusDto dto)
    {
        var agent = await _agentService.GetByIdAsync(id);
        if (agent is null)
        {
            return NotFound(new { message = "The requested agent does not exist." });
        }

        await _agentService.ChangeStatusAsync(id, dto.IsActive);
        return NoContent();
    }
}

public class ChangeAgentStatusDto
{
    public bool IsActive { get; set; }
}