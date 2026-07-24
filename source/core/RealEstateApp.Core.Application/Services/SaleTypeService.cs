using AutoMapper;
using RealEstateApp.Core.Application.Dtos.Catalog;
using RealEstateApp.Core.Application.Interfaces.Persistence;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Services;

public class SaleTypeService : ISaleTypeService
{
    private readonly ISaleTypeRepository _repository;
    private readonly IMapper _mapper;

    public SaleTypeService(ISaleTypeRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<SaleTypeDto>> GetAllAsync()
    {
        var list = await _repository.GetAllWithIncludeAsync(new List<string> { nameof(SaleType.Properties) });
        return _mapper.Map<List<SaleTypeDto>>(list);
    }

    public async Task<SaleTypeDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdWithIncludeAsync(id, new List<string> { nameof(SaleType.Properties) });
        return entity is null ? null : _mapper.Map<SaleTypeDto>(entity);
    }

    public async Task<SaleTypeDto> CreateAsync(SaleTypeDto dto)
    {
        var entity = _mapper.Map<SaleType>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<SaleTypeDto>(created);
    }

    public async Task<SaleTypeDto> UpdateAsync(int id, SaleTypeDto dto)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("The requested sale type does not exist.");

        entity.Name = dto.Name;
        entity.Description = dto.Description;

        await _repository.UpdateAsync(id, entity);
        return _mapper.Map<SaleTypeDto>(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdWithIncludeAsync(id, new List<string> { nameof(SaleType.Properties) })
            ?? throw new KeyNotFoundException("The requested sale type does not exist.");

        if (entity.Properties.Count > 0)
            throw new InvalidOperationException("This sale type cannot be deleted because it has properties associated with it.");

        await _repository.DeleteAsync(id);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        return await _repository.ExistsByNameAsync(name, excludeId);
    }

    public async Task<int> GetPropertiesCountAsync(int id)
    {
        var entity = await _repository.GetByIdWithIncludeAsync(id, new List<string> { nameof(SaleType.Properties) });
        return entity?.Properties.Count ?? 0;
    }
}