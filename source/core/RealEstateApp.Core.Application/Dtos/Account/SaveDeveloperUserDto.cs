namespace RealEstateApp.Core.Application.Dtos.Account;

public class SaveDeveloperUserDto
{
    public string? Id { get; set; }
    public string UserName { get; set; } = default!;
    public string FirstName { get; set; } = default!;
    public string LastName { get; set; } = default!;
    public string Email { get; set; } = default!;
    public string? Phone { get; set; }
    public string? Password { get; set; }
}