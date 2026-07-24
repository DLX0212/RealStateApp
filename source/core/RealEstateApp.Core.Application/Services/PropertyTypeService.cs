using AutoMapper;
using RealEstateApp.Core.Application.Dtos.Catalog;
using RealEstateApp.Core.Application.Interfaces.Persistence;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Services;

public class PropertyTypeService : IPropertyTypeService
{
    private readonly IPropertyTypeRepository _repository;
    private readonly IMapper _mapper;

    public PropertyTypeService(IPropertyTypeRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<PropertyTypeDto>> GetAllAsync()
    {
        var list = await _repository.GetAllWithIncludeAsync(new List<string> { nameof(PropertyType.Properties) });
        return _mapper.Map<List<PropertyTypeDto>>(list);
    }

    public async Task<PropertyTypeDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdWithIncludeAsync(id, new List<string> { nameof(PropertyType.Properties) });
        return entity is null ? null : _mapper.Map<PropertyTypeDto>(entity);
    }

    public async Task<PropertyTypeDto> CreateAsync(PropertyTypeDto dto)
    {
        var entity = _mapper.Map<PropertyType>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<PropertyTypeDto>(created);
    }

    public async Task<PropertyTypeDto> UpdateAsync(int id, PropertyTypeDto dto)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("The requested property type does not exist.");

        entity.Name = dto.Name;
        entity.Description = dto.Description;

        await _repository.UpdateAsync(id, entity);
        return _mapper.Map<PropertyTypeDto>(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdWithIncludeAsync(id, new List<string> { nameof(PropertyType.Properties) })
            ?? throw new KeyNotFoundException("The requested property type does not exist.");

        if (entity.Properties.Count > 0)
            throw new InvalidOperationException("This property type cannot be deleted because it has properties associated with it.");

        await _repository.DeleteAsync(id);
    }
    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        return await _repository.ExistsByNameAsync(name, excludeId);
    }

    public async Task<int> GetPropertiesCountAsync(int id)
    {
        var entity = await _repository.GetByIdWithIncludeAsync(id, new List<string> { nameof(PropertyType.Properties) });
        return entity?.Properties.Count ?? 0;
    }
}