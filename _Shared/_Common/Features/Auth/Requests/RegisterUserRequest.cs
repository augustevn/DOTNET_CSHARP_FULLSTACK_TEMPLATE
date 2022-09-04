using _Common.Features.UserInfo;
using _Common.Features.UserInfo.Requests;

namespace _Common.Features.Auth.Requests;

public record RegisterUserRequest(UserInfoRequest UserInfo, string Password, string Language, string RedirectUrl);