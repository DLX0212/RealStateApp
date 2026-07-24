namespace RealEstateApp.Core.Application.Dtos.Offer;

public class SaveOfferDto
{
    public decimal Amount { get; set; }
    public int PropertyId { get; set; }
    public string ClientId { get; set; } = default!;
}