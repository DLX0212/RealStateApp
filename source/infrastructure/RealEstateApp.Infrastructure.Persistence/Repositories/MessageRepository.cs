using Microsoft.EntityFrameworkCore;
using RealEstateApp.Core.Application.Interfaces.Persistence;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Infrastructure.Persistence.Context;

namespace RealEstateApp.Infrastructure.Persistence.Repositories;

public class MessageRepository : GenericRepository<Message>, IMessageRepository
{
    public MessageRepository(RealEstateAppContext context) : base(context)
    {
    }

    public async Task<List<Message>> GetConversationAsync(int propertyId, string clientId, string agentId)
    {
        return await _dbSet
            .Where(m => m.PropertyId == propertyId && m.ClientId == clientId && m.AgentId == agentId)
            .OrderBy(m => m.CreatedAt)
            .ToListAsync();
    }

    public async Task<List<string>> GetClientIdsAsync(int propertyId, string agentId)
    {
        return await _dbSet
            .Where(m => m.PropertyId == propertyId && m.AgentId == agentId)
            .Select(m => m.ClientId)
            .Distinct()
            .ToListAsync();
    }
}