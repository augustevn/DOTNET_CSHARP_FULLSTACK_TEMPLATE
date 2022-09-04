namespace _Common.Features.Auth.Requests;

public record ResendConfirmationEmailRequest(string Email, string RedirectUrl);