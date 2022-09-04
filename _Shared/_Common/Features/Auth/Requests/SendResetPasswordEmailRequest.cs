namespace _Common.Features.Auth.Requests;

public record SendResetPasswordEmailRequest(string Email, string RedirectUrl);