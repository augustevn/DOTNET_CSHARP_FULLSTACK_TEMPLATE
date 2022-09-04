using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;

namespace SharedApi.Config;

public static class JwtBearerConfig
{
    public static TokenValidationParameters GetTokenValidationParameters(string jwtSecret) {
        return new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.ASCII.GetBytes(jwtSecret)),
            ValidateIssuer = false,
            ValidateAudience = false,
            RequireExpirationTime = true,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero // Default is set to expire only 5 minutes after actual expiration, this sets it to 0.
        };
    }

    public static void ConfigureJwtBearerOptions(JwtBearerOptions options, string jwtSecret)
    {
        options.SaveToken = true;
        options.TokenValidationParameters = GetTokenValidationParameters(jwtSecret);
    }
}