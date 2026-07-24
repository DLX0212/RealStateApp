using AutoMapper;
using RealEstateApp.Core.Application.Dtos.Chat;
using RealEstateApp.Core.Application.Interfaces.Persistence;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Services;

public class MessageService : IMessageService
{
    private readonly IMessageRepository _repository;
    private readonly IMapper _mapper;

    public MessageService(IMessageRepository repository, IMapper mapper)
    {
        _repository = repository;
        _mapper = mapper;
    }

    public async Task<List<MessageDto>> GetConversationAsync(int propertyId, string clientId, string agentId)
    {
        var list = await _repository.GetConversationAsync(propertyId, clientId, agentId);
        return _mapper.Map<List<MessageDto>>(list);
    }

    public async Task<List<string>> GetClientsWithConversationAsync(int propertyId, string agentId)
    {
        return await _repository.GetClientIdsAsync(propertyId, agentId);
    }

    public async Task<MessageDto> SendAsync(MessageDto dto)
    {
        var entity = new Message
        {
            Content = dto.Content,
            SenderRole = dto.SenderRole,
            ClientId = dto.ClientId,
            AgentId = dto.AgentId,
            PropertyId = dto.PropertyId
        };

        var created = await _repository.AddAsync(entity);
        return _mapper.Map<MessageDto>(created);
    }
}