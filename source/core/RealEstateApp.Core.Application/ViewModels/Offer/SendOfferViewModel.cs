using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Offer;

public class SendOfferViewModel
{
    public int PropertyId { get; set; }

    [Required(ErrorMessage = "Please enter an offer amount.")]
    [Range(0.01, double.MaxValue, ErrorMessage = "The offer amount must be greater than zero.")]
    public decimal Amount { get; set; }
}