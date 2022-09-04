using System.Net.Http.Headers;
using System.Net.Http.Json;
using _Common.Constants;
using _Common.Features.Auth.Requests;
using _Common.Features.Auth.Requests.LoginUser;
using _Common.Features.Auth.Responses;
using Blazored.LocalStorage;
using Microsoft.Extensions.Logging;
using SharedSpa.Constants;
using SharedSpa.Interfaces;

namespace SharedSpa.Services;

public class AuthService : ISpaAuthService
{
    private const string BaseUrl = ApiRoutes.AuthEndpoint;
    
    public List<string>? CurrentUserRoles { get; set; }
    public string? CurrentUserId { get; set; }
    
    public event Func<Task> OnAuthStateChange;
    private void AuthStateChanged() => OnAuthStateChange.Invoke();
    
    public AuthTokenResponse? AuthTokens { get; set; }

    private readonly HttpClient _http;
    private readonly ILocalStorageService _localStorage;
    private readonly ILogger<AuthService> _logger;
    public AuthService(HttpClient http, ILocalStorageService localStorage, ILogger<AuthService> logger)
    {
        _http = http;
        _localStorage = localStorage;
        _logger = logger;
    }
    
    public bool IsLoggedIn => !string.IsNullOrEmpty(CurrentUserId);

    public async Task InitAuthState(bool forceFetch = false)
    {
        await GetAuthTokensFromLocalStorage();
        await FetchCurrentUser(forceFetch);

        AuthStateChanged();
    }
    
    public async Task FetchCurrentUser(bool forceFetch = false)
    {
        if (AuthTokens == null || (IsLoggedIn && !forceFetch))
        {
            return;
        }
        
        SetAuthHeader(AuthTokens.AccessToken);
        
        try
        {
            var authResponse = await _http.GetFromJsonAsync<CurrentUserResponse>(ApiRoutes.AuthMeEndpoint);
            SetCurrentUser(authResponse);
        }
        catch (HttpRequestException ex)
        {
            if (ex.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogDebug("Could not get current user, attempting to refresh tokens now...");

                // Try to refresh tokens.
                await RefreshTokens();
                
                try
                {
                    var authResponse = await _http.GetFromJsonAsync<CurrentUserResponse>(ApiRoutes.AuthMeEndpoint);
                    SetCurrentUser(authResponse);
                    
                    _logger.LogDebug("Successfully refreshed tokens and fetched user");
                }
                catch
                {
                    _logger.LogWarning("Could not get current user after refreshing tokens");
                }
            }
        }
    }
    
    public async Task LogUserIn(LoginUserRequest request)
    {
        var response = await _http.PostAsJsonAsync(BaseUrl, request);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Could not sign user in");
            return;
        }
        
        await ParseAuthResponse(response);
        await InitAuthState();
    }

    public async Task GetAuthTokensFromLocalStorage()
    {
        AuthTokens ??= await _localStorage.GetItemAsync<AuthTokenResponse>(LocalStorageKeys.TokensKey);
    }

    public async Task RefreshTokens()
    {
        var response = await _http.PostAsJsonAsync($"{BaseUrl}/Refresh", AuthTokens);
        if (!response.IsSuccessStatusCode)
        {
            _logger.LogWarning("Could not refresh auth tokens");

            // Invalid refresh token so unset.
            await UnsetAuthState();
            return;
        }
        
        await ParseAuthResponse(response);
    }

    public async Task LogUserOut()
    {
        await UnsetAuthState();
    }

    private void SetCurrentUser(CurrentUserResponse? authResponse)
    {
        CurrentUserId = authResponse?.UserId;
        CurrentUserRoles = authResponse?.UserRoles;
    }

    private async Task UnsetAuthState()
    {
        await _localStorage.RemoveItemAsync(LocalStorageKeys.TokensKey);
        AuthTokens = null;
        CurrentUserId = null;
        CurrentUserRoles = null;
        SetAuthHeader(null);
        
        AuthStateChanged();
    }

    private async Task ParseAuthResponse(HttpResponseMessage response)
    {
        var authSuccessResponse = await response.Content.ReadFromJsonAsync<AuthTokenResponse>();
        if (authSuccessResponse == null)
        {
            _logger.LogWarning("Failed to read authentication result"); // Shouldn't happen.
            return;
        }

        AuthTokens = new AuthTokenResponse(authSuccessResponse.AccessToken, authSuccessResponse.RefreshToken);

        await _localStorage.SetItemAsync(LocalStorageKeys.TokensKey, AuthTokens);
        SetAuthHeader(AuthTokens.AccessToken);
    }

    private void SetAuthHeader(string? accessToken)
    {
        _http.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", accessToken);
    }
    
    
    #region ROLE ACCESS
    
    public bool UserHasAccess(string role)
    {
        var hasRole = CurrentUserRoles != null && CurrentUserRoles.Contains(role);
        
        return hasRole;
    }

    public bool UserIsAdmin => UserHasAccess(CustomUserRoles.Admin);
    public bool UserIsModerator => UserHasAccess(CustomUserRoles.Moderator);
    public bool UserIsAppUser => UserHasAccess(CustomUserRoles.AppUser);

    public bool HasAdminAccess() => UserIsAdmin; // Need to be methods to pass as component parameter and invoke afterwards.
    public bool HasModeratorAccess() => UserIsAdmin || UserIsModerator;
    public bool HasAppUserAccess() => HasModeratorAccess() || UserIsAppUser;

    #endregion
}