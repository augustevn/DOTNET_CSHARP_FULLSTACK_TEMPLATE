namespace SharedApi.Interfaces;

public interface ICurrentUserService
{
    string? UserId { get; }
    List<string>? UserRoles { get; }
}