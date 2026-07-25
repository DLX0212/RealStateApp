using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using RealEstateApp.Core.Application.Dtos.Account;
using RealEstateApp.Core.Application.Interfaces.Services;

namespace RealEstateApp.WebApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AccountController : ControllerBase
{
    private readonly IAccountService _accountService;

    public AccountController(IAccountService accountService)
    {
        _accountService = accountService;
    }

    [HttpPost("login")]
    [AllowAnonymous]
    public async Task<IActionResult> Login([FromBody] LoginDto dto)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var result = await _accountService.LoginAsync(dto);

        if (!result.IsAuthenticated)
        {
            return Unauthorized(new { message = result.Message });
        }

        return Ok(result);
    }
    [HttpPost("register-developer")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RegisterDeveloper([FromBody] RegisterAdminDeveloperDto dto)
    {
        dto.RoleName = "Developer";
        return await RegisterAdminOrDeveloper(dto);
    }

    [HttpPost("register-admin")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> RegisterAdmin([FromBody] RegisterAdminDeveloperDto dto)
    {
        dto.RoleName = "Admin";
        return await RegisterAdminOrDeveloper(dto);
    }
    private async Task<IActionResult> RegisterAdminOrDeveloper(RegisterAdminDeveloperDto dto)
    {
        ModelState.Remove(nameof(dto.RoleName));

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            if (await _accountService.ExistsByEmailAsync(dto.Email))
            {
                return BadRequest(new { message = "A user is already registered with this email address." });
            }

            if (await _accountService.ExistsByUserNameAsync(dto.UserName))
            {
                return BadRequest(new { message = "A user is already registered with this username." });
            }

            if (await _accountService.ExistsByCedulaAsync(dto.Cedula))
            {
                return BadRequest(new { message = "A user is already registered with this cedula." });
            }

            var user = await _accountService.RegisterAdminOrDeveloperAsync(dto);
            return CreatedAtAction(nameof(Login), new { }, user);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}