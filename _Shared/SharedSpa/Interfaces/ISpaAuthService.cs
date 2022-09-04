using _Common.Features.Auth.Requests.LoginUser;
using _Common.Features.Auth.Responses;

namespace SharedSpa.Interfaces;

public interface ISpaAuthService
{
    AuthTokenResponse AuthTokens { get; set; }
    List<string>? CurrentUserRoles { get; set; }
    string? CurrentUserId { get; set; }
    event Func<Task> OnAuthStateChange;
    
    bool IsLoggedIn { get; }
    Task InitAuthState(bool forceFetch = false);
    
    Task FetchCurrentUser(bool forceFetch = false);
    Task GetAuthTokensFromLocalStorage();
    Task RefreshTokens();
    Task LogUserIn(LoginUserRequest user);
    Task LogUserOut();

    bool UserHasAccess(string role);
    bool UserIsAdmin { get; }
    bool UserIsModerator { get; }
    bool UserIsAppUser { get; }

    bool HasAdminAccess();
    bool HasModeratorAccess();
    bool HasAppUserAccess();
}