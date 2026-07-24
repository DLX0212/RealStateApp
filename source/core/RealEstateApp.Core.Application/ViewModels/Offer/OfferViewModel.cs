namespace RealEstateApp.Core.Application.ViewModels.Offer;

public class OfferViewModel
{
    public int Id { get; set; }
    public int PropertyId { get; set; }
    public decimal Amount { get; set; }
    public string Status { get; set; } = default!;
    public DateTime CreatedAt { get; set; }
}