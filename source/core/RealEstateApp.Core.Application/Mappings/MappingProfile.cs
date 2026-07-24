using AutoMapper;
using RealEstateApp.Core.Application.Dtos.Agent;
using RealEstateApp.Core.Application.Dtos.Catalog;
using RealEstateApp.Core.Application.Dtos.Chat;
using RealEstateApp.Core.Application.Dtos.Offer;
using RealEstateApp.Core.Application.Dtos.Property;
using RealEstateApp.Core.Application.ViewModels.Catalog;
using RealEstateApp.Core.Application.ViewModels.Chat;
using RealEstateApp.Core.Application.ViewModels.Offer;
using RealEstateApp.Core.Application.ViewModels.Property;
using RealEstateApp.Core.Domain.Entities;

namespace RealEstateApp.Core.Application.Mappings;

public class MappingProfile : Profile
{
    public MappingProfile()
    {
        // Catalog: Entity <-> Dto <-> ViewModel
        CreateMap<PropertyType, PropertyTypeDto>()
            .ForMember(d => d.PropertiesCount, o => o.MapFrom(s => s.Properties.Count))
            .ReverseMap();
        CreateMap<PropertyTypeDto, PropertyTypeViewModel>().ReverseMap();

        CreateMap<SaleType, SaleTypeDto>()
            .ForMember(d => d.PropertiesCount, o => o.MapFrom(s => s.Properties.Count))
            .ReverseMap();
        CreateMap<SaleTypeDto, SaleTypeViewModel>().ReverseMap();

        CreateMap<Improvement, ImprovementDto>()
            .ForMember(d => d.PropertiesCount, o => o.MapFrom(s => s.PropertyImprovements.Count))
            .ReverseMap();
        CreateMap<ImprovementDto, ImprovementViewModel>().ReverseMap();

        // Property
        CreateMap<Property, PropertyDto>()
            .ForMember(d => d.PropertyTypeName, o => o.MapFrom(s => s.PropertyType.Name))
            .ForMember(d => d.SaleTypeName, o => o.MapFrom(s => s.SaleType.Name))
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ForMember(d => d.Improvements, o => o.MapFrom(s => s.PropertyImprovements.Select(pi => pi.Improvement.Name)))
            .ForMember(d => d.Images, o => o.MapFrom(s => s.Images))
            .ForMember(d => d.AgentFullName, o => o.Ignore())
            .ForMember(d => d.AgentPhone, o => o.Ignore())
            .ForMember(d => d.AgentEmail, o => o.Ignore())
            .ForMember(d => d.AgentProfilePicturePath, o => o.Ignore());

        CreateMap<PropertyImage, PropertyImageDto>().ReverseMap();

        CreateMap<PropertyDto, PropertyListItemViewModel>()
            .ForMember(d => d.MainImagePath, o => o.MapFrom(s => s.Images.Select(i => i.Path).FirstOrDefault()))
            .ForMember(d => d.IsFavorite, o => o.Ignore());

        CreateMap<PropertyDto, PropertyDetailsViewModel>()
            .ForMember(d => d.ImagePaths, o => o.MapFrom(s => s.Images.Select(i => i.Path)))
            .ForMember(d => d.IsFavorite, o => o.Ignore())
            .ForMember(d => d.Messages, o => o.Ignore())
            .ForMember(d => d.Offers, o => o.Ignore())
            .ForMember(d => d.CanCreateOffer, o => o.Ignore())
            .ForMember(d => d.NewMessage, o => o.Ignore())
            .ForMember(d => d.NewOffer, o => o.Ignore())
            .ForMember(d => d.OfferBlockedReason, o => o.Ignore());

        CreateMap<PropertyDto, DeletePropertyViewModel>()
            .ForMember(d => d.MainImagePath, o => o.MapFrom(s => s.Images.Select(i => i.Path).FirstOrDefault()));

        CreateMap<SavePropertyViewModel, SavePropertyDto>()
            .ForMember(d => d.ImprovementIds, o => o.MapFrom(s => s.SelectedImprovementIds))
            .ForMember(d => d.ImagePaths, o => o.Ignore())
            .ForMember(d => d.AgentId, o => o.Ignore());

        // Agent
        // CreateMap<Agent, AgentDto>().ReverseMap();

        // Chat
        CreateMap<Message, MessageDto>().ReverseMap();
        CreateMap<MessageDto, ConversationViewModel>().ReverseMap();

        // Offer
        CreateMap<Offer, OfferDto>()
            .ForMember(d => d.Status, o => o.MapFrom(s => s.Status.ToString()))
            .ReverseMap()
            .ForMember(d => d.Status, o => o.Ignore());
        CreateMap<OfferDto, OfferViewModel>().ReverseMap();
    }
}