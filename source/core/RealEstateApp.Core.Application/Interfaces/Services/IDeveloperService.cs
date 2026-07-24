using RealEstateApp.Core.Application.Dtos.Account;

namespace RealEstateApp.Core.Application.Interfaces.Services;

public interface IDeveloperService
{
    Task<List<DeveloperUserDto>> GetAllAsync();
    Task<DeveloperUserDto?> GetByIdAsync(string id);
    Task CreateAsync(SaveDeveloperUserDto dto);
    Task UpdateAsync(SaveDeveloperUserDto dto);
    Task ChangeStatusAsync(string id, bool isActive);
}