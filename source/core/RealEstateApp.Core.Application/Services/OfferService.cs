using AutoMapper;
using RealEstateApp.Core.Application.Dtos.Offer;
using RealEstateApp.Core.Application.Interfaces.Persistence;
using RealEstateApp.Core.Application.Interfaces.Services;
using RealEstateApp.Core.Domain.Entities;
using RealEstateApp.Core.Domain.Enums;

namespace RealEstateApp.Core.Application.Services;

public class OfferService : IOfferService
{
    private readonly IOfferRepository _offerRepository;
    private readonly IPropertyRepository _propertyRepository;
    private readonly IMapper _mapper;

    public OfferService(IOfferRepository offerRepository, IPropertyRepository propertyRepository, IMapper mapper)
    {
        _offerRepository = offerRepository;
        _propertyRepository = propertyRepository;
        _mapper = mapper;
    }

    public async Task<List<OfferDto>> GetByPropertyAndClientAsync(int propertyId, string clientId)
    {
        var list = await _offerRepository.GetByPropertyAndClientAsync(propertyId, clientId);
        return _mapper.Map<List<OfferDto>>(list);
    }

    public async Task<List<OfferDto>> GetByPropertyAsync(int propertyId)
    {
        var list = await _offerRepository.GetByPropertyAsync(propertyId);
        return _mapper.Map<List<OfferDto>>(list);
    }

    public async Task<OfferDto> CreateAsync(SaveOfferDto dto)
    {
        if (dto.Amount <= 0)
            throw new InvalidOperationException("The offer amount must be greater than zero.");

        var property = await _propertyRepository.GetByIdAsync(dto.PropertyId)
            ?? throw new KeyNotFoundException("The requested property does not exist.");

        if (property.Status == PropertyStatus.Sold)
            throw new InvalidOperationException("This property has already been sold and cannot receive new offers.");

        var acceptedOffer = await _offerRepository.GetAcceptedByPropertyAsync(dto.PropertyId);
        if (acceptedOffer is not null)
            throw new InvalidOperationException("This property already has an accepted offer.");

        var pendingOffer = await _offerRepository.GetPendingByClientAndPropertyAsync(dto.ClientId, dto.PropertyId);
        if (pendingOffer is not null)
            throw new InvalidOperationException("You already have a pending offer for this property.");

        var entity = new Offer
        {
            Amount = dto.Amount,
            PropertyId = dto.PropertyId,
            ClientId = dto.ClientId,
            Status = OfferStatus.Pending
        };

        var created = await _offerRepository.AddAsync(entity);
        return _mapper.Map<OfferDto>(created);
    }

    public async Task AcceptAsync(int offerId)
    {
        var offer = await _offerRepository.GetByIdAsync(offerId)
            ?? throw new KeyNotFoundException("The requested offer does not exist.");

        if (offer.Status != OfferStatus.Pending)
            throw new InvalidOperationException("This offer has already been answered.");

        offer.Status = OfferStatus.Accepted;
        await _offerRepository.UpdateAsync(offer.Id, offer);

        var otherPending = await _offerRepository.GetPendingByPropertyAsync(offer.PropertyId);
        foreach (var pending in otherPending.Where(o => o.Id != offer.Id))
        {
            pending.Status = OfferStatus.Rejected;
            await _offerRepository.UpdateAsync(pending.Id, pending);
        }

        var property = await _propertyRepository.GetByIdAsync(offer.PropertyId);
        if (property is not null)
        {
            property.Status = PropertyStatus.Sold;
            await _propertyRepository.UpdateAsync(property.Id, property);
        }
    }

    public async Task RejectAsync(int offerId)
    {
        var offer = await _offerRepository.GetByIdAsync(offerId)
            ?? throw new KeyNotFoundException("The requested offer does not exist.");

        if (offer.Status != OfferStatus.Pending)
            throw new InvalidOperationException("This offer has already been answered.");

        offer.Status = OfferStatus.Rejected;
        await _offerRepository.UpdateAsync(offer.Id, offer);
    }
}