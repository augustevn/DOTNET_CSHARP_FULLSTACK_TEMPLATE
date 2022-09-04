using Blazored.LocalStorage;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedSpa.Interfaces;
using SharedSpa.Services;

namespace SharedSpa;

public static class ConfigureServices
{
    public static void AddSharedSpa(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddBlazoredLocalStorage();
        
        services.AddScoped<ISpaNavigationBaseService, NavigationBaseService>();
        services.AddScoped<ISpaAuthService, AuthService>();
    }
}