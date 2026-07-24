using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Application.Dtos.Account;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Services;

public class DeveloperService : IDeveloperService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private const string DeveloperRole = "Developer";

    public DeveloperService(UserManager<ApplicationUser> userManager)
    {
        _userManager = userManager;
    }

    private static DeveloperUserDto MapToDto(ApplicationUser user) => new()
    {
        Id = user.Id,
        UserName = user.UserName ?? string.Empty,
        FirstName = user.FirstName,
        LastName = user.LastName,
        Email = user.Email ?? string.Empty,
        Phone = user.Phone,
        IsActive = user.IsActive
    };

    public async Task<List<DeveloperUserDto>> GetAllAsync()
    {
        var developers = (await _userManager.GetUsersInRoleAsync(DeveloperRole))
            .OrderBy(a => a.FirstName)
            .ThenBy(a => a.LastName);

        return developers.Select(MapToDto).ToList();
    }

    public async Task<DeveloperUserDto?> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(DeveloperRole)) return null;

        return MapToDto(user);
    }

    public async Task CreateAsync(SaveDeveloperUserDto dto)
    {
        var existingUserName = await _userManager.FindByNameAsync(dto.UserName);
        if (existingUserName is not null)
            throw new InvalidOperationException("The username is already taken.");

        var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
        if (existingEmail is not null)
            throw new InvalidOperationException("The email is already registered.");

        var user = new ApplicationUser
        {
            UserName = dto.UserName,
            Email = dto.Email,
            EmailConfirmed = true,
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Phone = dto.Phone,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(user, dto.Password!);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException(errors);
        }

        await _userManager.AddToRoleAsync(user, DeveloperRole);
    }

    public async Task UpdateAsync(SaveDeveloperUserDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.Id!)
            ?? throw new KeyNotFoundException("The requested developer does not exist.");

        var roles = await _userManager.GetRolesAsync(user);
        if (!roles.Contains(DeveloperRole))
            throw new KeyNotFoundException("The requested developer does not exist.");

        if (!string.Equals(user.UserName, dto.UserName, StringComparison.OrdinalIgnoreCase))
        {
            var existingUserName = await _userManager.FindByNameAsync(dto.UserName);
            if (existingUserName is not null && existingUserName.Id != user.Id)
                throw new InvalidOperationException("The username is already taken.");
        }

        if (!string.Equals(user.Email, dto.Email, StringComparison.OrdinalIgnoreCase))
        {
            var existingEmail = await _userManager.FindByEmailAsync(dto.Email);
            if (existingEmail is not null && existingEmail.Id != user.Id)
                throw new InvalidOperationException("The email is already registered.");
        }

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Phone = dto.Phone;

        var setUserNameResult = await _userManager.SetUserNameAsync(user, dto.UserName);
        if (!setUserNameResult.Succeeded)
            throw new InvalidOperationException(string.Join(" ", setUserNameResult.Errors.Select(e => e.Description)));

        var setEmailResult = await _userManager.SetEmailAsync(user, dto.Email);
        if (!setEmailResult.Succeeded)
            throw new InvalidOperationException(string.Join(" ", setEmailResult.Errors.Select(e => e.Description)));

        var updateResult = await _userManager.UpdateAsync(user);
        if (!updateResult.Succeeded)
            throw new InvalidOperationException(string.Join(" ", updateResult.Errors.Select(e => e.Description)));

        if (!string.IsNullOrWhiteSpace(dto.Password))
        {
            var token = await _userManager.GeneratePasswordResetTokenAsync(user);
            var pwdResult = await _userManager.ResetPasswordAsync(user, token, dto.Password);
            if (!pwdResult.Succeeded)
                throw new InvalidOperationException(string.Join(" ", pwdResult.Errors.Select(e => e.Description)));
        }
    }

    public async Task ChangeStatusAsync(string id, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new KeyNotFoundException("The requested developer does not exist.");

        user.IsActive = isActive;
        await _userManager.UpdateAsync(user);
    }
}