using RealEstateApp.Core.Application.Dtos.Common;

namespace RealEstateApp.Core.Application.Dtos.Offer;

public class OfferDto : BasicDto
{
    public decimal Amount { get; set; }
    public string Status { get; set; } = default!;
    public string ClientId { get; set; } = default!;
    public int PropertyId { get; set; }
    public DateTime CreatedAt { get; set; }
}