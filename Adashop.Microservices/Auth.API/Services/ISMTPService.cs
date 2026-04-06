namespace Auth.API.Services;

public interface ISMTPService
{
    Task SendEmailAsync( string toEmail, string subject, string body );
}