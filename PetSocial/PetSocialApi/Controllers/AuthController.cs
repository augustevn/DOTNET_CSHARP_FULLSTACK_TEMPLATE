using _Common.Constants;
using _Common.Features.Auth.Requests;
using _Common.Features.Auth.Requests.LoginUser;
using _Common.Features.Auth.Responses;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using SharedApi.Interfaces;

namespace PetSocialApi.Controllers;

[ApiController]
[Route("[controller]")]
public class AuthController : Controller
{
    private readonly IApiAuthService _authService;
    private readonly ICurrentUserService _currentUserService;
    public AuthController(IApiAuthService authService, ICurrentUserService currentUserService)
    {
        _authService = authService;
        _currentUserService = currentUserService;
    }

    #region GET
    
    [HttpGet("Me")]
    [Authorize]
    public IActionResult GetCurrentUser()
    {
        return Ok(new CurrentUserResponse(_currentUserService.UserRoles, _currentUserService.UserId));
    }
    
    #endregion

    
    #region POST
    
    [HttpPost]
    public async Task<IActionResult> Login([FromBody] LoginUserRequest requestBody)
    {
        var authResponse = await _authService.Login(requestBody);
        return Ok(authResponse);
    }

    [HttpPost("Register")]
    public async Task<IActionResult> Register([FromBody] RegisterUserRequest requestBody)
    {
        await _authService.Register(requestBody);
        return NoContent();
    }
    
    [HttpPost("Confirm")]
    public async Task<IActionResult> ConfirmUserProfile([FromBody] ConfirmUserRequest requestBody)
    {
        await _authService.ConfirmUser(requestBody);
        return NoContent();
    }
    
    [HttpPost("Refresh")]
    public async Task<IActionResult> RefreshToken([FromBody] RefreshTokenRequest requestBody)
    {
        var authResponse = await _authService.RefreshTokens(requestBody);
        return Ok(authResponse);
    }
    
    [HttpPost("ResendConfirmationEmail")]
    public async Task<IActionResult> ResendConfirmationEmail([FromBody] ResendConfirmationEmailRequest requestBody)
    {
        await _authService.ResendConfirmationEmail(requestBody);
        return NoContent();
    }

    [HttpPost("SendResetPasswordEmail")]
    public async Task<IActionResult> SendResetPasswordEmail([FromBody] SendResetPasswordEmailRequest requestBody)
    {
        await _authService.SendResetPasswordEmail(requestBody);
        return NoContent();
    }
        
    [HttpPost("ResetPassword")]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest requestBody)
    {
        await _authService.ResetPassword(requestBody);
        return NoContent();
    }

    #endregion
    
    
    #region DELETE

    [HttpDelete("Delete")]
    [Authorize]
    public async Task<IActionResult> DeleteUser()
    {
        await _authService.DeleteUser(_currentUserService.UserId);
        return NoContent();
    }

    #endregion
}