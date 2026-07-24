using RealEstateApp.Core.Application.Dtos.Catalog;

namespace RealEstateApp.Core.Application.Interfaces.Services;

public interface ISaleTypeService
{
    Task<List<SaleTypeDto>> GetAllAsync();
    Task<SaleTypeDto?> GetByIdAsync(int id);
    Task<SaleTypeDto> CreateAsync(SaleTypeDto dto);
    Task<SaleTypeDto> UpdateAsync(int id, SaleTypeDto dto);
    Task DeleteAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<int> GetPropertiesCountAsync(int id);
}