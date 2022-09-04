using _Common.Features.Auth.Requests;

namespace SharedApi.Interfaces;

public interface IApiAuthService : IApiAuthBaseService
{
    Task Register(RegisterUserRequest requestBody);
    Task ResendConfirmationEmail(ResendConfirmationEmailRequest requestBody);
    Task SendResetPasswordEmail(SendResetPasswordEmailRequest requestBody);
    Task DeleteUser(string userId);
}