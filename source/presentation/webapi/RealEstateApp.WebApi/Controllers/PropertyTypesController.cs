using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Dtos.Catalog;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PropertyTypesController : ControllerBase
{
    private readonly IPropertyTypeService _propertyTypeService;

    public PropertyTypesController(IPropertyTypeService propertyTypeService)
    {
        _propertyTypeService = propertyTypeService;
    }
    [HttpGet]
    [Authorize(Roles = "Admin,Developer")]
    public async Task<IActionResult> GetAll()
    {
        var items = await _propertyTypeService.GetAllAsync();

        if (items.Count == 0)
        {
            return NoContent();
        }

        return Ok(items);
    }

    [HttpGet("{id:int}")]
    [Authorize(Roles = "Admin,Developer")]
    public async Task<IActionResult> GetById(int id)
    {
        var item = await _propertyTypeService.GetByIdAsync(id);

        if (item is null)
        {
            return NotFound(new { message = "The requested property type does not exist." });
        }

        return Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] PropertyTypeDto dto)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Description))
        {
            return BadRequest(new { message = "The submitted data is not valid." });
        }

        if (await _propertyTypeService.ExistsByNameAsync(dto.Name))
        {
            return BadRequest(new { message = "A property type is already registered with this name." });
        }

        var created = await _propertyTypeService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] PropertyTypeDto dto)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Description))
        {
            return BadRequest(new { message = "The submitted data is not valid." });
        }

        var existing = await _propertyTypeService.GetByIdAsync(id);
        if (existing is null)
        {
            return NotFound(new { message = "The requested property type does not exist." });
        }

        if (await _propertyTypeService.ExistsByNameAsync(dto.Name, id))
        {
            return BadRequest(new { message = "Another property type is already registered with this name." });
        }

        var updated = await _propertyTypeService.UpdateAsync(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _propertyTypeService.GetByIdAsync(id);
        if (existing is null)
        {
            return NotFound(new { message = "The requested property type does not exist." });
        }

        await _propertyTypeService.DeleteAsync(id);
        return NoContent();
    }
}