namespace _Common.Features.Auth.Responses;

public record AuthTokenResponse(string? AccessToken = null, string? RefreshToken = null);