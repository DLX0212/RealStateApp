using RealEstateApp.Core.Application.Dtos.Account;

namespace RealEstateApp.Core.Application.Interfaces.Services;

public interface IAccountService
{
    Task<JwtDto> LoginAsync(LoginDto dto);
    Task<UserDto> RegisterClientOrAgentAsync(RegisterClientAgentDto dto, string? activationBaseUrl);
    Task<UserDto> RegisterAdminOrDeveloperAsync(RegisterAdminDeveloperDto dto);
    Task<UserDto?> GetByIdAsync(string id);
    Task<List<UserDto>> GetByRoleAsync(string roleName);
    Task<UserDto> UpdateAsync(UserDto dto);
    Task<bool> ActivateAccountAsync(string userId, string token);
    Task ChangeStatusAsync(string id, bool isActive);
    Task<bool> ExistsByEmailAsync(string email, string? excludeId = null);
    Task<bool> ExistsByUserNameAsync(string userName, string? excludeId = null);
    Task<bool> ExistsByCedulaAsync(string cedula, string? excludeId = null);
    Task<int> CountActiveByRoleAsync(string roleName);
    Task<int> CountInactiveByRoleAsync(string roleName);
}