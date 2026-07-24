using RealEstateApp.Core.Application.ViewModels.Chat;
using RealEstateApp.Core.Application.ViewModels.Offer;

namespace RealEstateApp.Core.Application.ViewModels.Property;

public class PropertyDetailsViewModel
{
    public int Id { get; set; }
    public string Code { get; set; } = default!;
    public string PropertyTypeName { get; set; } = default!;
    public string SaleTypeName { get; set; } = default!;
    public decimal Price { get; set; }
    public decimal Size { get; set; }
    public int Bedrooms { get; set; }
    public int Bathrooms { get; set; }
    public string Description { get; set; } = default!;
    public string Status { get; set; } = default!;

    public List<string> ImagePaths { get; set; } = new();
    public List<string> Improvements { get; set; } = new();

    public string AgentId { get; set; } = default!;
    public string AgentFullName { get; set; } = default!;
    public string? AgentPhone { get; set; }
    public string? AgentEmail { get; set; }
    public string? AgentProfilePicturePath { get; set; }

    public bool IsFavorite { get; set; }

    public List<ConversationViewModel> Messages { get; set; } = new();
    public SendMessageViewModel NewMessage { get; set; } = new();

    public List<OfferViewModel> Offers { get; set; } = new();
    public SendOfferViewModel NewOffer { get; set; } = new();
    public bool CanCreateOffer { get; set; }
    public string? OfferBlockedReason { get; set; }
}