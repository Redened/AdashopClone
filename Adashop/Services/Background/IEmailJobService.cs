namespace Adashop.Services.Background;

public interface IEmailJobService
{
    Task SendInactiveEmails();
}
