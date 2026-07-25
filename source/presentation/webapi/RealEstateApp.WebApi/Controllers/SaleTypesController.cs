using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Dtos.Catalog;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SaleTypesController : ControllerBase
{
    private readonly ISaleTypeService _saleTypeService;

    public SaleTypesController(ISaleTypeService saleTypeService)
    {
        _saleTypeService = saleTypeService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Developer")]
    public async Task<IActionResult> GetAll()
    {
        var items = await _saleTypeService.GetAllAsync();

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
        var item = await _saleTypeService.GetByIdAsync(id);

        if (item is null)
        {
            return NotFound(new { message = "The requested sale type does not exist." });
        }

        return Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] SaleTypeDto dto)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Description))
        {
            return BadRequest(new { message = "The submitted data is not valid." });
        }

        if (await _saleTypeService.ExistsByNameAsync(dto.Name))
        {
            return BadRequest(new { message = "A sale type is already registered with this name." });
        }

        var created = await _saleTypeService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] SaleTypeDto dto)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Description))
        {
            return BadRequest(new { message = "The submitted data is not valid." });
        }

        var existing = await _saleTypeService.GetByIdAsync(id);
        if (existing is null)
        {
            return NotFound(new { message = "The requested sale type does not exist." });
        }

        if (await _saleTypeService.ExistsByNameAsync(dto.Name, id))
        {
            return BadRequest(new { message = "Another sale type is already registered with this name." });
        }

        var updated = await _saleTypeService.UpdateAsync(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _saleTypeService.GetByIdAsync(id);
        if (existing is null)
        {
            return NotFound(new { message = "The requested sale type does not exist." });
        }

        await _saleTypeService.DeleteAsync(id);
        return NoContent();
    }
}