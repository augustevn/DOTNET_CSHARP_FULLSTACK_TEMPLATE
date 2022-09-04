using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using System.Web;
using _Common.Features.Auth.Requests;
using _Common.Features.Auth.Requests.LoginUser;
using _Common.Features.Auth.Responses;
using _Common.Features.Mailing.Requests;
using Mailing.Interfaces;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using SharedApi.Config;
using SharedApi.Entities;
using SharedApi.Exceptions;
using SharedApi.Interfaces;
using JwtRegisteredClaimNames = Microsoft.IdentityModel.JsonWebTokens.JwtRegisteredClaimNames;
using ValidationFailure = FluentValidation.Results.ValidationFailure;

namespace SharedApi.Services;

public class AuthBaseService : IApiAuthBaseService
{

    private readonly UserManager<IdentityUser> _userManager;
    private readonly SignInManager<IdentityUser> _signInManager;
    private readonly ILogger<IApiAuthBaseService> _logger;
    private readonly IDbContext _context;
    private readonly IWebHostEnvironment _environment;
    private readonly IEmailService _emailService;
    private readonly JwtConfig _jwtConfig;
    protected AuthBaseService(UserManager<IdentityUser> userManager, ILogger<IApiAuthBaseService> logger, SignInManager<IdentityUser> signInManager, IDbContext context, IWebHostEnvironment environment, IEmailService emailService, IOptions<JwtConfig> jwtConfig)
    {
        _userManager = userManager;
        _logger = logger;
        _signInManager = signInManager;
        _context = context;
        _environment = environment;
        _emailService = emailService;
        _jwtConfig = jwtConfig.Value;
    }
    
    #region LOG USER IN

    public async Task<AuthTokenResponse> Login(LoginUserRequest requestBody)
    {
        var existingUser = await _userManager.FindByEmailAsync(requestBody.Email);
        if (existingUser == null)
        {
            _logger.LogWarning($"User does not exist: [{requestBody.Email}]");
            throw new ForbiddenAccessException();
        }

        // Checks for valid credentials and whether the account is confirmed. Only then an access token should be issued.
        var result = await _signInManager.CheckPasswordSignInAsync(existingUser, requestBody.Password, false);
        if (!result.Succeeded)
        {
            _logger.LogWarning($"Invalid credentials or inactive account: [{requestBody.Email}]");
            throw new ForbiddenAccessException();
        }

        return await GenerateAuthResponse(existingUser);
    }

    #endregion
    
    
    #region CREATE USER

    public async Task<UserProfile> CreateUserProfile(IdentityUser newUser, string name)
    {
        var newUserProfile = new UserProfile
        {
            LinkedUserId = newUser.Id,
            UserInfo = new UserInfo
            {
                Email = newUser.Email,
                Name = name,
                PhoneNumber = newUser.PhoneNumber
            }
        };

        await _context.UserProfiles.AddAsync(newUserProfile);
        await _context.SaveChangesAsync(new CancellationToken());

        return newUserProfile;
    }
    
    #endregion

    
    #region CONFIRM USER

    public async Task ConfirmUser(ConfirmUserRequest requestBody)
    {
        var existingUser = await _userManager.FindByIdAsync(requestBody.UserId);
        if (existingUser == null)
        {
            throw new NotFoundException("Could not find user to confirm");
        }
        
        var result = await _userManager.ConfirmEmailAsync(existingUser, requestBody.Token);
        if (!result.Succeeded)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("AuthError", "Failed to confirm user, try again later")
            });
        }

        var existingUserProfile = await _context.UserProfiles.FirstOrDefaultAsync(userProfile => userProfile.LinkedUserId == requestBody.UserId);
        if (existingUserProfile == null)
        {
            throw new NotFoundException(nameof(UserProfile), requestBody.UserId);
        }

        existingUserProfile.IsEmailConfirmed = true;

        _context.UserProfiles.Update(existingUserProfile);
        await _context.SaveChangesAsync(new CancellationToken());
    }
    
    public async Task ResendConfirmationEmail(ResendConfirmationEmailRequest requestBody)
    {
        var existingUser = await _userManager.FindByEmailAsync(requestBody.Email);
        if (existingUser == null)
        {
            throw new NotFoundException(
                $"Could not find user to resend confirmation e-mail for: [{requestBody.Email}]");
        }

        if (existingUser.EmailConfirmed)
        {
            throw new ForbiddenAccessException(); // User did already confirm.
        }

        await SendConfirmUserMail(existingUser, requestBody.RedirectUrl);
    }
    
    public async Task SendConfirmUserMail(IdentityUser user, string endpointUrl)
    {
        var token = await _userManager.GenerateEmailConfirmationTokenAsync(user);
        var url = $"{endpointUrl}/{user.Id}/{HttpUtility.UrlEncode(token)}";

        var emailBody = $"<a href=\"{url}\">Activeer hier uw account</a>.";

        const string contactType = "Account activatie";
        // var (subject, body) = StringHelper.ConstructAuthEmailTemplate(contactType, emailBody, "applicatie");

        await SendEmail(new MailRequest(contactType, emailBody, user.Email));
    }

    #endregion


    #region RESET PASSWORD

    public async Task SendResetPasswordEmail(SendResetPasswordEmailRequest requestBody)
    {
        var existingUser = await _userManager.FindByEmailAsync(requestBody.Email);
        if (existingUser == null)
        {
            throw new NotFoundException(
                $"Could not find user to send reset password e-mail for: [{requestBody.Email}]");
        }

        await SendResetPasswordMail(existingUser, requestBody.RedirectUrl);
    }

    private async Task SendResetPasswordMail(IdentityUser user, string endpointUrl)
    {
        var token = await _userManager.GeneratePasswordResetTokenAsync(user);
        var url = $"{endpointUrl}/{user.Id}/{HttpUtility.UrlEncode(token)}";

        var emailBody = $"<a href=\"{url}\">Herstel hier uw wachtwoord</a>.";
        
        const string contactType = "Wachtwoord herstel";
        // var (subject, body) = StringHelper.ConstructAuthEmailTemplate(contactType, emailBody, "applicatie");
        
        await SendEmail(new MailRequest(contactType, emailBody, user.Email));
    }
    
    public async Task ResetPassword(ResetPasswordRequest requestBody)
    {
        var existingUser = await _userManager.FindByIdAsync(requestBody.UserId);
        if (existingUser == null)
        {
            throw new NotFoundException($"Could not find user to reset password for: [{requestBody.UserId}]");
        }

        var result = await _userManager.ResetPasswordAsync(existingUser, requestBody.Token, requestBody.Password);
        if (!result.Succeeded)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("AuthError", "Failed to reset user password, try again later")
            });
        }
    }

    #endregion


    #region DELETE USER

    protected async Task DeleteUser(string userId)
    {
        var existingUser = await _userManager.FindByIdAsync(userId);
        if (existingUser == null)
        {
            throw new NotFoundException("Could not find user to delete");
        }

        try
        {
            // Delete user and everything related to it
            var tokens = await _context.RefreshTokens
                .Where(token => token.LinkedUserId == userId)
                .AsNoTracking()
                .ToListAsync();

            var existingUserProfile = await _context.UserProfiles
                .Include(userProfile => userProfile.UserInfo)
                .AsNoTracking()
                .FirstOrDefaultAsync(userProfile => userProfile.LinkedUserId == userId);

            _context.RefreshTokens.RemoveRange(tokens);
            _context.UserInfos.Remove(existingUserProfile.UserInfo);
            _context.UserProfiles.Remove(existingUserProfile);

            await _context.SaveChangesAsync(new CancellationToken());
            await _userManager.DeleteAsync(existingUser);
        }
        catch (Exception _)
        {
            throw new ValidationException(new[]
            {
                new ValidationFailure("AuthError", "Could not delete user")
            });
        }
    }

    #endregion

    
    #region SEND MAIL
    
    private async Task SendEmail(MailRequest mailEnvelope)
    {
        if (_environment.IsDevelopment())
        {
            mailEnvelope = mailEnvelope with { To = "wijaugust@gmail.com" };
        }

        var (subject, body, to, replyTo) = mailEnvelope;
        
        await _emailService.SendEmail(subject, body, to, replyTo);
    }
    
    #endregion
    

    #region REFRESH JWT TOKEN

    public async Task<AuthTokenResponse> RefreshTokens(RefreshTokenRequest requestBody)
    {
        var validatedAccessToken = GetClaimsPrincipal(requestBody.AccessToken);
        if (validatedAccessToken == null)
        {
            _logger.LogWarning("Invalid access token");
            throw new ForbiddenAccessException();
        }

        var jwtId = validatedAccessToken.Claims.FirstOrDefault(claim => claim.Type == JwtRegisteredClaimNames.Jti)
            ?.Value;

        var storedRefreshToken = await _context.RefreshTokens.FirstOrDefaultAsync(refresh => refresh.JwtId == requestBody.RefreshToken);
        if (storedRefreshToken == null)
        {
            _logger.LogWarning("This refresh token does not exist");
            throw new ForbiddenAccessException();
        }

        if (DateTime.UtcNow > storedRefreshToken.ExpiresAt || storedRefreshToken.IsInvalidated ||
            storedRefreshToken.IsUsed || storedRefreshToken.JwtId != jwtId)
        {
            _logger.LogWarning("Invalid refresh token");
            throw new ForbiddenAccessException();
        }

        var user = await _userManager.FindByIdAsync(validatedAccessToken.Claims.FirstOrDefault(claim => claim.Type == ClaimTypes.NameIdentifier)?.Value);
        if (user == null)
        {
            _logger.LogWarning("Could not find user for validatedAccessToken");
            throw new ForbiddenAccessException();
        }

        storedRefreshToken.IsUsed = true;

        _context.RefreshTokens.Update(storedRefreshToken);
        await _context.SaveChangesAsync(new CancellationToken());

        return await GenerateAuthResponse(user);
    }
    
    private ClaimsPrincipal? GetClaimsPrincipal(string accessToken)
    {
        var tokenHandler = new JwtSecurityTokenHandler();

        try
        {
            var tokenValidationParams = JwtBearerConfig.GetTokenValidationParameters(_jwtConfig.Secret).Clone();
            tokenValidationParams.ValidateLifetime = false;

            var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParams, out var validatedAccessToken);
            return HasValidAlgorithm(validatedAccessToken) ? principal : null;
        }
        catch
        {
            return null;
        }
    }

    private static bool HasValidAlgorithm(SecurityToken validatedAccessToken)
    {
        return (validatedAccessToken is JwtSecurityToken jwtSecurityToken) && jwtSecurityToken.Header.Alg
            .Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase);
    }

    #endregion

    
    #region CREATE JWT TOKEN

    private async Task<AuthTokenResponse> GenerateAuthResponse(IdentityUser user)
    {
        var jwtId = Guid.NewGuid().ToString();

        var refreshToken = new RefreshToken
        {
            JwtId = jwtId,
            LinkedUserId = user.Id,
            ExpiresAt = DateTime.UtcNow.AddMonths(6)
        };

        await _context.RefreshTokens.AddAsync(refreshToken);
        await _context.SaveChangesAsync(new CancellationToken());
        
        var userRoles = await _userManager.GetRolesAsync(user);
        var claimsWithRoles = userRoles.Select(role => new Claim(ClaimTypes.Role, role));
        var accessToken = CreateJwtToken(user.Id, jwtId, claimsWithRoles);

        return new AuthTokenResponse(accessToken, refreshToken.JwtId);
    }
    
    private string CreateJwtToken(string userId, string jwtId, IEnumerable<Claim> claims)
    {
        var secret = Encoding.ASCII.GetBytes(_jwtConfig.Secret);

        var allClaims = claims.ToList();
        allClaims.Add(new Claim(ClaimTypes.NameIdentifier, userId)); // 'JwtRegisteredClaimNames.Sub' does not get mapped properly.
        allClaims.Add(new Claim(JwtRegisteredClaimNames.Jti, jwtId));
        
        var tokenDescriptor = new SecurityTokenDescriptor
        {
            Subject = new ClaimsIdentity(allClaims),
            Expires = DateTime.UtcNow.Add(_jwtConfig.TTL),
            SigningCredentials =
                new SigningCredentials(new SymmetricSecurityKey(secret), SecurityAlgorithms.HmacSha256Signature)
        };

        var tokenHandler = new JwtSecurityTokenHandler();
        var token = tokenHandler.CreateToken(tokenDescriptor);

        return tokenHandler.WriteToken(token);
    }

    #endregion
}