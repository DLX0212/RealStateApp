using AutoMapper;
using RealEstateApp.Core.Application.Dtos.Property;
using RealEstateApp.Core.Application.Interfaces.Persistence;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Core.Application.Services;

public class PropertyService : IPropertyService
{
    private readonly IPropertyRepository _repository;
    private readonly IAccountService _accountService;
    private readonly IMapper _mapper;

    public PropertyService(IPropertyRepository repository, IAccountService accountService, IMapper mapper)
    {
        _repository = repository;
        _accountService = accountService;
        _mapper = mapper;
    }

    private async Task<PropertyDto> MapWithAgentAsync(Property property)
    {
        var dto = _mapper.Map<PropertyDto>(property);
        var agent = await _accountService.GetByIdAsync(property.AgentId);

        if (agent is not null)
        {
            dto.AgentFullName = $"{agent.FirstName} {agent.LastName}";
            dto.AgentPhone = agent.Phone;
            dto.AgentEmail = agent.Email;
            dto.AgentProfilePicturePath = agent.ProfilePicturePath;
        }

        return dto;
    }

    private async Task<List<PropertyDto>> MapWithAgentAsync(List<Property> properties)
    {
        var result = new List<PropertyDto>();
        foreach (var property in properties)
        {
            result.Add(await MapWithAgentAsync(property));
        }
        return result;
    }

    private async Task<List<PropertyDto>> FilterByActiveAgentAsync(List<PropertyDto> properties)
    {
        var result = new List<PropertyDto>();
        foreach (var property in properties)
        {
            var agent = await _accountService.GetByIdAsync(property.AgentId);
            if (agent is not null && agent.IsActive)
            {
                result.Add(property);
            }
        }
        return result;
    }

    public async Task<List<PropertyDto>> GetAllAsync()
    {
        var list = await _repository.GetAllWithIncludeAsync(new List<string>
        {
            nameof(Property.PropertyType), nameof(Property.SaleType), nameof(Property.Images)
        });
        return await MapWithAgentAsync(list);
    }

    public async Task<List<PropertyDto>> GetAvailableAsync()
    {
        var list = await _repository.GetAvailableAsync();
        var mapped = await MapWithAgentAsync(list);
        return await FilterByActiveAgentAsync(mapped);
    }

    public async Task<List<PropertyDto>> GetAvailableFilteredAsync(PropertyFilterDto filter)
    {
        var list = await _repository.GetAvailableFilteredAsync(filter);
        var mapped = await MapWithAgentAsync(list);
        return await FilterByActiveAgentAsync(mapped);
    }

    public async Task<PropertyDto?> GetAvailableByCodeAsync(string code)
    {
        var entity = await _repository.GetAvailableByCodeAsync(code);
        if (entity is null) return null;

        var dto = await MapWithAgentAsync(entity);
        var agent = await _accountService.GetByIdAsync(dto.AgentId);
        return (agent is not null && agent.IsActive) ? dto : null;
    }

    public async Task<PropertyDto?> GetByIdAsync(int id)
    {
        var entity = await _repository.GetWithDetailsAsync(id);
        return entity is null ? null : await MapWithAgentAsync(entity);
    }

    public async Task<List<PropertyDto>> GetByAgentIdAsync(string agentId)
    {
        var list = await _repository.GetByAgentIdAsync(agentId);
        return await MapWithAgentAsync(list);
    }

    public async Task<List<PropertyDto>> GetAvailableByAgentIdAsync(string agentId)
    {
        var list = await _repository.GetAvailableByAgentIdAsync(agentId);
        return await MapWithAgentAsync(list);
    }

    public async Task<PropertyDto> CreateAsync(SavePropertyDto dto)
    {
        var code = await _repository.GenerateUniqueCodeAsync();

        var entity = new Property
        {
            Code = code,
            PropertyTypeId = dto.PropertyTypeId,
            SaleTypeId = dto.SaleTypeId,
            Price = dto.Price,
            Description = dto.Description,
            Size = dto.Size,
            Bedrooms = dto.Bedrooms,
            Bathrooms = dto.Bathrooms,
            AgentId = dto.AgentId,
            Status = PropertyStatus.Available,
            Images = dto.ImagePaths.Select(path => new PropertyImage { Path = path }).ToList(),
            PropertyImprovements = dto.ImprovementIds
                .Select(improvementId => new PropertyImprovement { ImprovementId = improvementId })
                .ToList()
        };

        var created = await _repository.AddAsync(entity);
        var withDetails = await _repository.GetWithDetailsAsync(created.Id);
        return await MapWithAgentAsync(withDetails!);
    }

    public async Task<PropertyDto> UpdateAsync(int id, SavePropertyDto dto)
    {
        var entity = await _repository.GetWithDetailsAsync(id)
            ?? throw new KeyNotFoundException("The requested property does not exist.");

        if (entity.Status == PropertyStatus.Sold)
            throw new InvalidOperationException("A property that has already been sold cannot be modified.");

        entity.PropertyTypeId = dto.PropertyTypeId;
        entity.SaleTypeId = dto.SaleTypeId;
        entity.Price = dto.Price;
        entity.Description = dto.Description;
        entity.Size = dto.Size;
        entity.Bedrooms = dto.Bedrooms;
        entity.Bathrooms = dto.Bathrooms;

        entity.PropertyImprovements.Clear();
        foreach (var improvementId in dto.ImprovementIds)
        {
            entity.PropertyImprovements.Add(new PropertyImprovement { PropertyId = id, ImprovementId = improvementId });
        }

        if (dto.ImagePaths.Count > 0)
        {
            entity.Images.Clear();
            foreach (var path in dto.ImagePaths)
            {
                entity.Images.Add(new PropertyImage { PropertyId = id, Path = path });
            }
        }

        await _repository.UpdateAsync(id, entity);

        var updated = await _repository.GetWithDetailsAsync(id);
        return await MapWithAgentAsync(updated!);
    }

    public async Task DeleteAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("The requested property does not exist.");

        if (entity.Status == PropertyStatus.Sold)
            throw new InvalidOperationException("A property that has already been sold cannot be deleted.");

        await _repository.DeleteAsync(id);
    }

    public async Task MarkAsSoldAsync(int id)
    {
        var entity = await _repository.GetByIdAsync(id)
            ?? throw new KeyNotFoundException("The requested property does not exist.");

        entity.Status = PropertyStatus.Sold;
        await _repository.UpdateAsync(id, entity);
    }
}