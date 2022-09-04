using _Common.Constants;
using _Common.Features.Auth.Requests;
using FluentValidation.Results;
using Mailing.Interfaces;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;
using SharedApi.Config;
using SharedApi.Exceptions;
using SharedApi.Interfaces;
using SharedApi.Services;

namespace PetSocialApi.Services;

public class AuthService : AuthBaseService, IApiAuthService
{
    private readonly UserManager<IdentityUser> _userManager;
    private readonly ILogger<IApiAuthBaseService> _logger;

    public AuthService(UserManager<IdentityUser> userManager, ILogger<IApiAuthBaseService> logger, SignInManager<IdentityUser> signInManager, IDbContext context, IWebHostEnvironment environment, IEmailService emailService, IOptions<JwtConfig> jwtConfig) :
        base(userManager, logger, signInManager, context, environment, emailService, jwtConfig)
    {
        _userManager = userManager;
        _logger = logger;
    }

    #region CREATE USER

    public async Task Register(RegisterUserRequest requestBody)
    {
        var existingUser = await _userManager.FindByEmailAsync(requestBody.UserInfo.Email);
        if (existingUser != null)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("AuthError", "Email address already in use")
            });
        }

        var newUser = new IdentityUser
        {
            Email = requestBody.UserInfo.Email,
            UserName = requestBody.UserInfo.Email,
            PhoneNumber = requestBody.UserInfo.PhoneNumber,
        };

        try
        {
            var createdUserResult = await _userManager.CreateAsync(newUser, requestBody.Password);
            if (!createdUserResult.Succeeded)
            {
                throw new ValidationException(createdUserResult.Errors.Select(error =>
                    new ValidationFailure("AuthError", error.Description)));
            }
            
            await _userManager.AddToRoleAsync(newUser, CustomUserRoles.AppUser);
            
            var createdUserProfile = await CreateUserProfile(newUser, requestBody.UserInfo.Name);
                
            if (createdUserProfile == null)
            {
                throw new ValidationException(new[]
                {
                    new ValidationFailure("AuthError","Could not create new user profile, try again later")
                });
            }
            
            await SendConfirmUserMail(newUser, requestBody.RedirectUrl);
        }
        catch (Exception e)
        {
            _logger.LogWarning($"Could not register user, exception: [{e}]");

            throw new ValidationException(new[]
            {
                new ValidationFailure("AuthError", "Could not register user")
            });
        }
    }

    #endregion

    #region DELETE

    public async Task DeleteUser(string userId)
    {
        await base.DeleteUser(userId);
    }

    #endregion
}