using Microsoft.AspNetCore.Identity;

namespace SharedApi.Config;

public static class IdentityConfig
{
    public static void ConfigureIdentityOptions(IdentityOptions options)
    {
        options.Password = new PasswordOptions
        {
            RequireLowercase = false,
            RequireUppercase = false,
            RequireNonAlphanumeric = false,
            RequireDigit = false
        };

        options.User.RequireUniqueEmail = true;
        options.SignIn.RequireConfirmedEmail = true;
    }
}