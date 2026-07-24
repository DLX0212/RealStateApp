using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Chat;

public class SendMessageViewModel
{
    public int PropertyId { get; set; }
    public string? ClientId { get; set; }

    [Required(ErrorMessage = "Please write a message before sending.")]
    [StringLength(1000, ErrorMessage = "The message cannot exceed 1000 characters.")]
    public string Content { get; set; } = default!;
}