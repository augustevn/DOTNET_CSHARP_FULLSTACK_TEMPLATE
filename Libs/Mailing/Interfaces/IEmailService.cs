namespace Mailing.Interfaces;

public interface IEmailService
{
    Task SendEmail(string subject, string body, string to, string? replyTo = null);
}