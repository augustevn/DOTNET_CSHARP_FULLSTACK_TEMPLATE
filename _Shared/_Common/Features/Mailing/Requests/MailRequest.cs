namespace _Common.Features.Mailing.Requests;

public record MailRequest(string Subject, string Body, string To = "wijaugust@gmail.com", string? ReplyTo = null);