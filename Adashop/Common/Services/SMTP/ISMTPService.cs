namespace Adashop.Common.Services.SMTP;

public interface ISMTPService
{
    Task SendEmailAsync( string toEmail, string subject, string body );
}