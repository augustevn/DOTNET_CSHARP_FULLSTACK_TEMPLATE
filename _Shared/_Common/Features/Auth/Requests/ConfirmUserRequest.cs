namespace _Common.Features.Auth.Requests;

public record ConfirmUserRequest(string UserId, string Token);