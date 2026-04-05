using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace Adashop.Common.Services.SMTP;

public class SMTPService : ISMTPService
{
    private readonly IConfiguration _CONFIGURATION;

    public SMTPService( IConfiguration configuration ) => _CONFIGURATION = configuration;


    public async Task SendEmailAsync( string email, string subject, string body )
    {
        if ( !MailboxAddress.TryParse(email, out var toEmail) )
            throw new InvalidOperationException("Recipient email address is invalid");


        var fromEmail = _CONFIGURATION["SMTP:FromEmail"] ?? throw new InvalidOperationException("SMTP:FromEmail is not configured");
        var appPassword = _CONFIGURATION["SMTP:AppPassword"] ?? throw new InvalidOperationException("SMTP:AppPassword is not configured");
        var host = _CONFIGURATION["SMTP:Host"] ?? throw new InvalidOperationException("SMTP:Host is not configured");
        if ( !int.TryParse(_CONFIGURATION["SMTP:Port"], out var port) )
            throw new InvalidOperationException("SMTP:Port is not configured or invalid");


        var message = new MimeMessage();

        message.From.Add(new MailboxAddress("Adashop", fromEmail));
        message.To.Add(toEmail);
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = body };


        using var smtp = new SmtpClient();
        smtp.Timeout = 30000;

        try
        {
            await smtp.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await smtp.AuthenticateAsync(fromEmail, appPassword);
            await smtp.SendAsync(message);
        }
        finally
        {
            if ( smtp.IsConnected )
                await smtp.DisconnectAsync(true);
        }
    }
}
