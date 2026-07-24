using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Persistence;

public interface IMessageRepository : IGenericRepository<Message>
{
    Task<List<Message>> GetConversationAsync(int propertyId, string clientId, string agentId);
    Task<List<string>> GetClientIdsAsync(int propertyId, string agentId);
}