using Adashop.Services.Background;
using Hangfire;

namespace Adashop.Extensions;

public static class ApplicationExtensions
{
    public static WebApplication ExtendApplication( this WebApplication app )
    {
        if ( app.Environment.IsDevelopment() )
        {
            app.UseSwagger();
            app.UseSwaggerUI();
        }

        app.UseHangfireDashboard();

        RecurringJob.AddOrUpdate<IEmailJobService>("send-email-to-inactive-users", job => job.SendInactiveEmails(), Cron.Daily);

        app.UseHttpsRedirection();
        app.UseAuthentication();
        app.UseAuthorization();
        app.MapControllers();

        return app;
    }
}