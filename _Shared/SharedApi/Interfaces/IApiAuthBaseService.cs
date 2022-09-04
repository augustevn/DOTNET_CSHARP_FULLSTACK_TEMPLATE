using _Common.Features.Auth.Requests;
using _Common.Features.Auth.Requests.LoginUser;
using _Common.Features.Auth.Responses;

namespace SharedApi.Interfaces;

public interface IApiAuthBaseService
{
    Task<AuthTokenResponse> Login(LoginUserRequest requestBody);
    Task<AuthTokenResponse> RefreshTokens(RefreshTokenRequest requestBody);
    Task ConfirmUser(ConfirmUserRequest requestBody);
    Task ResendConfirmationEmail(ResendConfirmationEmailRequest requestBody);
    Task ResetPassword(ResetPasswordRequest requestBody);
    Task SendResetPasswordEmail(SendResetPasswordEmailRequest requestBody);
}