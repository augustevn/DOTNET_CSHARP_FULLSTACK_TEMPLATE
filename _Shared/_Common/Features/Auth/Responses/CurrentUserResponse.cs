namespace _Common.Features.Auth.Responses;

public record CurrentUserResponse(List<string>? UserRoles = null, string? UserId = null);