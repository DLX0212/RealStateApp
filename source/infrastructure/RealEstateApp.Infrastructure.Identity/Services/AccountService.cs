using Microsoft.AspNetCore.Identity;
using RealEstateApp.Core.Application.Dtos.Account;
using RealEstateApp.Core.Application.Interfaces.Infrastructure;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Infrastructure.Identity.Entities;

namespace RealEstateApp.Infrastructure.Identity.Services;

public class AccountService : IAccountService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly IJwtService _jwtService;
    private readonly IEmailService _emailService;

    public AccountService(UserManager<ApplicationUser> userManager, IJwtService jwtService, IEmailService emailService)
    {
        _userManager = userManager;
        _jwtService = jwtService;
        _emailService = emailService;
    }

    private static UserDto MapToDto(ApplicationUser user, string role)
    {
        return new UserDto
        {
            Id = user.Id,
            FirstName = user.FirstName,
            LastName = user.LastName,
            UserName = user.UserName ?? string.Empty,
            Email = user.Email ?? string.Empty,
            Phone = user.Phone,
            ProfilePicturePath = user.ProfilePicturePath,
            Cedula = user.Cedula,
            IsActive = user.IsActive,
            Role = role
        };
    }

    public async Task<JwtDto> LoginAsync(LoginDto dto)
    {
        var user = await _userManager.FindByEmailAsync(dto.UserNameOrEmail)
                   ?? await _userManager.FindByNameAsync(dto.UserNameOrEmail);

        if (user is null)
        {
            return new JwtDto { IsAuthenticated = false, Message = "The access data is invalid." };
        }

        var passwordValid = await _userManager.CheckPasswordAsync(user, dto.Password);
        if (!passwordValid)
        {
            return new JwtDto { IsAuthenticated = false, Message = "The access data is invalid." };
        }

        if (!user.IsActive)
        {
            return new JwtDto { IsAuthenticated = false, Message = "The user is inactive and cannot log in." };
        }

        var roles = await _userManager.GetRolesAsync(user);
        if (roles.Count == 0)
        {
            return new JwtDto { IsAuthenticated = false, Message = "The user does not have a valid role assigned." };
        }

        return await _jwtService.GenerateTokenAsync(user.Id, user.UserName ?? string.Empty, user.Email ?? string.Empty, roles.ToList());
    }

    public async Task<UserDto> RegisterClientOrAgentAsync(RegisterClientAgentDto dto, string? activationBaseUrl)
    {
        var user = new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Phone = dto.Phone,
            ProfilePicturePath = dto.ProfilePicturePath,
            UserName = dto.UserName,
            Email = dto.Email,
            IsActive = false
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"It was not possible to complete the registration. {errors}");
        }

        await _userManager.AddToRoleAsync(user, dto.RoleName);

        if (dto.RoleName == "Client" && !string.IsNullOrWhiteSpace(activationBaseUrl))
        {
            var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
            var encodedToken = Uri.EscapeDataString(token);
            var activationLink = $"{activationBaseUrl}?userId={user.Id}&token={encodedToken}";

            await _emailService.SendActivationEmailAsync(user.Email!, user.FirstName, activationLink);
        }

        return MapToDto(user, dto.RoleName);
    }

    public async Task<bool> ActivateAccountAsync(string userId, string token)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user is null) return false;

        var decodedToken = Uri.UnescapeDataString(token);
        var result = await _userManager.ConfirmEmailAsync(user, decodedToken);
        if (!result.Succeeded) return false;

        user.IsActive = true;
        await _userManager.UpdateAsync(user);
        return true;
    }

    public async Task<UserDto> RegisterAdminOrDeveloperAsync(RegisterAdminDeveloperDto dto)
    {
        var user = new ApplicationUser
        {
            FirstName = dto.FirstName,
            LastName = dto.LastName,
            Cedula = dto.Cedula,
            UserName = dto.UserName,
            Email = dto.Email,
            IsActive = true 
        };

        var result = await _userManager.CreateAsync(user, dto.Password);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"It was not possible to complete the registration. {errors}");
        }

        await _userManager.AddToRoleAsync(user, dto.RoleName);

        return MapToDto(user, dto.RoleName);
    }

    public async Task<UserDto?> GetByIdAsync(string id)
    {
        var user = await _userManager.FindByIdAsync(id);
        if (user is null) return null;

        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles.FirstOrDefault() ?? string.Empty);
    }

    public async Task<List<UserDto>> GetByRoleAsync(string roleName)
    {
        var users = await _userManager.GetUsersInRoleAsync(roleName);
        return users.Select(u => MapToDto(u, roleName)).ToList();
    }

    public async Task<UserDto> UpdateAsync(UserDto dto)
    {
        var user = await _userManager.FindByIdAsync(dto.Id)
            ?? throw new KeyNotFoundException("The requested user does not exist.");

        user.FirstName = dto.FirstName;
        user.LastName = dto.LastName;
        user.Phone = dto.Phone;
        user.Email = dto.Email;
        user.UserName = dto.UserName;
        user.Cedula = dto.Cedula;

        if (!string.IsNullOrWhiteSpace(dto.ProfilePicturePath))
        {
            user.ProfilePicturePath = dto.ProfilePicturePath;
        }

        var result = await _userManager.UpdateAsync(user);
        if (!result.Succeeded)
        {
            var errors = string.Join(" ", result.Errors.Select(e => e.Description));
            throw new InvalidOperationException($"It was not possible to update the user. {errors}");
        }

        var roles = await _userManager.GetRolesAsync(user);
        return MapToDto(user, roles.FirstOrDefault() ?? dto.Role);
    }

    public async Task ChangeStatusAsync(string id, bool isActive)
    {
        var user = await _userManager.FindByIdAsync(id)
            ?? throw new KeyNotFoundException("The requested user does not exist.");

        user.IsActive = isActive;
        await _userManager.UpdateAsync(user);
    }

    public async Task<bool> ExistsByEmailAsync(string email, string? excludeId = null)
    {
        var user = await _userManager.FindByEmailAsync(email);
        return user is not null && user.Id != excludeId;
    }

    public async Task<bool> ExistsByUserNameAsync(string userName, string? excludeId = null)
    {
        var user = await _userManager.FindByNameAsync(userName);
        return user is not null && user.Id != excludeId;
    }

    public async Task<bool> ExistsByCedulaAsync(string cedula, string? excludeId = null)
    {
        var user = _userManager.Users.FirstOrDefault(u => u.Cedula == cedula);
        return user is not null && user.Id != excludeId;
    }

    public async Task<int> CountActiveByRoleAsync(string roleName)
    {
        var users = await _userManager.GetUsersInRoleAsync(roleName);
        return users.Count(u => u.IsActive);
    }

    public async Task<int> CountInactiveByRoleAsync(string roleName)
    {
        var users = await _userManager.GetUsersInRoleAsync(roleName);
        return users.Count(u => !u.IsActive);
    }
}