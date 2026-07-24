using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Interfaces.Persistence;

public interface IOfferRepository : IGenericRepository<Offer>
{
    Task<List<Offer>> GetByPropertyAndClientAsync(int propertyId, string clientId);
    Task<List<Offer>> GetByPropertyAsync(int propertyId);
    Task<Offer?> GetPendingByClientAndPropertyAsync(string clientId, int propertyId);
    Task<Offer?> GetAcceptedByPropertyAsync(int propertyId);
    Task<List<Offer>> GetPendingByPropertyAsync(int propertyId);
}