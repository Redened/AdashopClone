using Adashop.Common.Services.SMTP;
using Adashop.Data;
using Microsoft.EntityFrameworkCore;

namespace Adashop.Services.Background;

public class EmailJobService : IEmailJobService
{
    private readonly DataContext _DB;
    private readonly ISMTPService _SMTP;

    public EmailJobService( DataContext DB, ISMTPService SMTP )
    {
        _DB = DB;
        _SMTP = SMTP;
    }

    public async Task SendInactiveEmails()
    {
        var cutoff = DateTime.UtcNow.AddDays(-30);

        var users = await _DB.Users
            .Where(u => u.LastLoginAt == null || u.LastLoginAt < cutoff)
            .ToListAsync();

        foreach ( var user in users )
        {
            await _SMTP.SendEmailAsync(user.Email, "You haven't logged in for a while", "Please do consider coming back. We miss you!");
        }
    }
}