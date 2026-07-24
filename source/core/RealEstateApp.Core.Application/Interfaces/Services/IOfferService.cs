using RealEstateApp.Core.Application.Dtos.Offer;

namespace RealEstateApp.Core.Application.Interfaces.Services;

public interface IOfferService
{
    Task<List<OfferDto>> GetByPropertyAndClientAsync(int propertyId, string clientId);
    Task<List<OfferDto>> GetByPropertyAsync(int propertyId);
    Task<OfferDto> CreateAsync(SaveOfferDto dto);
    Task AcceptAsync(int offerId);
    Task RejectAsync(int offerId);
}