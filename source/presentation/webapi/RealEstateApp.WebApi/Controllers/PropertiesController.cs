using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Dtos.Property;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PropertiesController : ControllerBase
{
    private readonly IPropertyService _propertyService;
    private readonly IPropertyTypeService _propertyTypeService;
    private readonly ISaleTypeService _saleTypeService;

    public PropertiesController(
        IPropertyService propertyService,
        IPropertyTypeService propertyTypeService,
        ISaleTypeService saleTypeService)
    {
        _propertyService = propertyService;
        _propertyTypeService = propertyTypeService;
        _saleTypeService = saleTypeService;
    }

    private string CurrentAgentId => User.FindFirstValue(ClaimTypes.NameIdentifier)!;

    [HttpGet]
    [Authorize(Roles = "Admin,Developer")]
    public async Task<IActionResult> GetAll([FromQuery] int? propertyTypeId,
        [FromQuery] decimal? minPrice,
        [FromQuery] decimal? maxPrice,
        [FromQuery] int? bedrooms,
        [FromQuery] int? bathrooms,
        [FromQuery] string? code)
    {
        var hasFilters = propertyTypeId.HasValue || minPrice.HasValue || maxPrice.HasValue
                         || bedrooms.HasValue || bathrooms.HasValue || !string.IsNullOrWhiteSpace(code);

        List<PropertyDto> properties;

        if (hasFilters)
        {
            var filter = new PropertyFilterDto
            {
                PropertyTypeId = propertyTypeId,
                MinPrice = minPrice,
                MaxPrice = maxPrice,
                Bedrooms = bedrooms,
                Bathrooms = bathrooms,
                Code = code
            };
            properties = await _propertyService.GetAvailableFilteredAsync(filter);
        }
        else
        {
            properties = await _propertyService.GetAllAsync();
        }

        if (properties.Count == 0)
        {
            return NoContent();
        }

        return Ok(properties);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Developer")]
    public async Task<IActionResult> GetById(int id)
    {
        var property = await _propertyService.GetByIdAsync(id);

        if (property is null)
        {
            return NotFound(new { message = "The requested property does not exist." });
        }

        return Ok(property);
    }

    [HttpGet("code/{code}")]
    [Authorize(Roles = "Admin,Developer")]
    public async Task<IActionResult> GetByCode(string code)
    {
        var property = await _propertyService.GetAvailableByCodeAsync(code);

        if (property is null)
        {
            return NotFound(new { message = "No property registered with the entered code was found." });
        }

        return Ok(property);
    }

    [HttpPost]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> Create([FromBody] SavePropertyDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        if (await _propertyTypeService.GetByIdAsync(dto.PropertyTypeId) is null)
        {
            return BadRequest(new { message = "The selected property type does not exist." });
        }

        if (await _saleTypeService.GetByIdAsync(dto.SaleTypeId) is null)
        {
            return BadRequest(new { message = "The selected sale type does not exist." });
        }

        dto.AgentId = CurrentAgentId;

        var created = await _propertyService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> Update(int id, [FromBody] SavePropertyDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var existing = await _propertyService.GetByIdAsync(id);
        if (existing is null)
        {
            return NotFound(new { message = "The requested property does not exist." });
        }

        if (existing.AgentId != CurrentAgentId)
        {
            return Forbid();
        }

        if (existing.Status == "Sold")
        {
            return BadRequest(new { message = "A property that has already been sold cannot be modified." });
        }

        if (await _propertyTypeService.GetByIdAsync(dto.PropertyTypeId) is null)
        {
            return BadRequest(new { message = "The selected property type does not exist." });
        }

        if (await _saleTypeService.GetByIdAsync(dto.SaleTypeId) is null)
        {
            return BadRequest(new { message = "The selected sale type does not exist." });
        }

        dto.Id = id;
        dto.AgentId = CurrentAgentId;

        var updated = await _propertyService.UpdateAsync(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Agent")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _propertyService.GetByIdAsync(id);
        if (existing is null)
        {
            return NotFound(new { message = "The requested property does not exist." });
        }

        if (existing.AgentId != CurrentAgentId)
        {
            return Forbid();
        }

        if (existing.Status == "Sold")
        {
            return BadRequest(new { message = "A property that has already been sold cannot be deleted." });
        }

        await _propertyService.DeleteAsync(id);
        return NoContent();
    }
}