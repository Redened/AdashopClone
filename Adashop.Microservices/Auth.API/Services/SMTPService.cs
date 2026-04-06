using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Auth.API.Services;

public class SMTPService : ISMTPService
{
    private readonly IConfiguration _CONFIGURATION;

    public SMTPService( IConfiguration configuration ) => _CONFIGURATION = configuration;

    public async Task SendEmailAsync( string email, string subject, string body )
    {
        if ( !MailboxAddress.TryParse(email, out var toEmail) )
            throw new InvalidOperationException("Recipient email address is invalid");


        var username = _CONFIGURATION["SMTP:Username"] ?? throw new InvalidOperationException("SMTP:Username is not configured");
        var password = _CONFIGURATION["SMTP:Password"] ?? throw new InvalidOperationException("SMTP:Password is not configured");
        var host = _CONFIGURATION["SMTP:Host"] ?? throw new InvalidOperationException("SMTP:Host is not configured");
        if ( !int.TryParse(_CONFIGURATION["SMTP:Port"], out var port) )
            throw new InvalidOperationException("SMTP:Port is not configured or invalid");


        var message = new MimeMessage();

        message.From.Add(new MailboxAddress("Adashop", username));
        message.To.Add(toEmail);
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };


        using var smtp = new SmtpClient();
        smtp.Timeout = 30000;

        try
        {
            await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(username, password);
            await smtp.SendAsync(message);
        }
        finally
        {
            if ( smtp.IsConnected )
                await smtp.DisconnectAsync(true);
        }
    }

}
