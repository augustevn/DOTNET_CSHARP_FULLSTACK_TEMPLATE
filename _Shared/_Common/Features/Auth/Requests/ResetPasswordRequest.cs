namespace _Common.Features.Auth.Requests;

public record ResetPasswordRequest(string UserId, string Token, string Password);