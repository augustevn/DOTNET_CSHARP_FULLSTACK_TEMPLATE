namespace _Common.Features.Auth.Requests;

public record RefreshTokenRequest(string AccessToken, string RefreshToken);