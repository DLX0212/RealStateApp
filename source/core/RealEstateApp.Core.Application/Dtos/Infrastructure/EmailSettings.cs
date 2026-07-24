namespace RealEstateApp.Core.Application.Settings;

public class EmailSettings
{
    public string SmtpHost { get; set; } = default!;
    public int SmtpPort { get; set; }
    public string SenderEmail { get; set; } = default!;
    public string SenderPassword { get; set; } = default!;
    public string SenderName { get; set; } = default!;
    public bool EnableSsl { get; set; } = true;
}