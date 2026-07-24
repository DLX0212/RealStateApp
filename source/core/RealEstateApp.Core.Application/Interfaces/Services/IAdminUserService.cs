using RealEstateApp.Core.Application.Dtos.Account;

namespace RealEstateApp.Core.Application.Interfaces.Services;

public interface IAdminUserService
{
    Task<List<AdminUserDto>> GetAllAsync();
    Task<AdminUserDto?> GetByIdAsync(string id);
    Task CreateAsync(SaveAdminUserDto dto);
    Task UpdateAsync(SaveAdminUserDto dto, string currentUserId);
    Task ChangeStatusAsync(string id, bool isActive, string currentUserId);
}