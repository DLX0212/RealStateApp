using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Catalog;

public class ImprovementViewModel
{
    public int Id { get; set; }

    [Required(ErrorMessage = "Name is required.")]
    [StringLength(50)]
    public string Name { get; set; } = default!;

    [StringLength(250)]
    public string? Description { get; set; }

    public int PropertiesCount { get; set; }
}