using System.ComponentModel.DataAnnotations;

namespace RealEstateApp.Core.Application.ViewModels.Account;

public class SaveDeveloperUserViewModel : IValidatableObject
{
    public string? Id { get; set; }

    [Required(ErrorMessage = "Username is required.")]
    [StringLength(50)]
    [Display(Name = "Username")]
    public string UserName { get; set; } = default!;

    [Required(ErrorMessage = "First name is required.")]
    [StringLength(50)]
    [Display(Name = "First Name")]
    public string FirstName { get; set; } = default!;

    [Required(ErrorMessage = "Last name is required.")]
    [StringLength(50)]
    [Display(Name = "Last Name")]
    public string LastName { get; set; } = default!;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "Invalid email format.")]
    public string Email { get; set; } = default!;

    [Phone(ErrorMessage = "Invalid phone format.")]
    public string? Phone { get; set; }

    [DataType(DataType.Password)]
    public string? Password { get; set; }

    [DataType(DataType.Password)]
    [Compare(nameof(Password), ErrorMessage = "Passwords do not match.")]
    [Display(Name = "Confirm Password")]
    public string? ConfirmPassword { get; set; }

    public IEnumerable<ValidationResult> Validate(ValidationContext validationContext)
    {
        var isCreate = string.IsNullOrEmpty(Id);

        if (isCreate && string.IsNullOrWhiteSpace(Password))
        {
            yield return new ValidationResult("Password is required.", new[] { nameof(Password) });
        }

        if (!string.IsNullOrWhiteSpace(Password) && Password.Length < 6)
        {
            yield return new ValidationResult("Password must be at least 6 characters.", new[] { nameof(Password) });
        }
    }
}