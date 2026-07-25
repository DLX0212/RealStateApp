using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Dtos.Catalog;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class ImprovementsController : ControllerBase
{
    private readonly IImprovementService _improvementService;

    public ImprovementsController(IImprovementService improvementService)
    {
        _improvementService = improvementService;
    }

    [HttpGet]
    [Authorize(Roles = "Admin,Developer")]
    public async Task<IActionResult> GetAll()
    {
        var items = await _improvementService.GetAllAsync();

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
        var item = await _improvementService.GetByIdAsync(id);

        if (item is null)
        {
            return NotFound(new { message = "The requested improvement does not exist." });
        }

        return Ok(item);
    }

    [HttpPost]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Create([FromBody] ImprovementDto dto)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Description))
        {
            return BadRequest(new { message = "The submitted data is not valid." });
        }

        if (await _improvementService.ExistsByNameAsync(dto.Name))
        {
            return BadRequest(new { message = "An improvement is already registered with this name." });
        }

        var created = await _improvementService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = created.Id }, created);
    }

    [HttpPut("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Update(int id, [FromBody] ImprovementDto dto)
    {
        if (!ModelState.IsValid || string.IsNullOrWhiteSpace(dto.Name) || string.IsNullOrWhiteSpace(dto.Description))
        {
            return BadRequest(new { message = "The submitted data is not valid." });
        }

        var existing = await _improvementService.GetByIdAsync(id);
        if (existing is null)
        {
            return NotFound(new { message = "The requested improvement does not exist." });
        }

        if (await _improvementService.ExistsByNameAsync(dto.Name, id))
        {
            return BadRequest(new { message = "Another improvement is already registered with this name." });
        }

        var updated = await _improvementService.UpdateAsync(id, dto);
        return Ok(updated);
    }

    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var existing = await _improvementService.GetByIdAsync(id);
        if (existing is null)
        {
            return NotFound(new { message = "The requested improvement does not exist." });
        }

        await _improvementService.DeleteAsync(id);
        return NoContent();
    }
}