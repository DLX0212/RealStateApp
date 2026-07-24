using RealEstateApp.Core.Application.Dtos.Catalog;

namespace RealEstateApp.Core.Application.Interfaces.Services;

public interface IPropertyTypeService
{
    Task<List<PropertyTypeDto>> GetAllAsync();
    Task<PropertyTypeDto?> GetByIdAsync(int id);
    Task<PropertyTypeDto> CreateAsync(PropertyTypeDto dto);
    Task<PropertyTypeDto> UpdateAsync(int id, PropertyTypeDto dto);
    Task DeleteAsync(int id);
    Task<bool> ExistsByNameAsync(string name, int? excludeId = null);
    Task<int> GetPropertiesCountAsync(int id);
}