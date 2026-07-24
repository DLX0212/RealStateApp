using AutoMapper;
using RealEstateApp.Core.Application.Dtos.Catalog;
using RealEstateApp.Core.Application.Interfaces.Persistence;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Services;

public class ImprovementService : IImprovementService
{
    private readonly IImprovementRepository _repository;
    private readonly IMapper _mapper;

    public ImprovementService(IImprovementRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<ImprovementDto>> GetAllAsync()
    {
        var list = await _repository.GetAllWithIncludeAsync(new List<string> { nameof(Improvement.PropertyImprovements) });
        return _mapper.Map<List<ImprovementDto>>(list);
    }

    public async Task<ImprovementDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetByIdWithIncludeAsync(id, new List<string> { nameof(Improvement.PropertyImprovements) });
        return entity is null ? null : _mapper.Map<ImprovementDto>(entity);
    }

    public async Task<ImprovementDto> CreateAsync(ImprovementDto dto)
    {
        var entity = _mapper.Map<Improvement>(dto);
        var created = await _repository.AddAsync(entity);
        return _mapper.Map<ImprovementDto>(created);
    }

    public async Task<ImprovementDto> UpdateAsync(int id, ImprovementDto dto)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("The requested improvement does not exist.");

        entity.Name = dto.Name;
        entity.Description = dto.Description;

        await _repository.UpdateAsync(id, entity);
        return _mapper.Map<ImprovementDto>(entity);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("The requested improvement does not exist.");

        await _repository.RemoveRelationsAsync(id);
        await _repository.DeleteAsync(id);
    }

    public async Task<bool> ExistsByNameAsync(string name, int? excludeId = null)
    {
        return await _repository.ExistsByNameAsync(name, excludeId);
    }
}