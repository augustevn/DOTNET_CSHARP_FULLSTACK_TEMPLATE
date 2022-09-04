using System.Reflection;
using FluentValidation.AspNetCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedApi.Attributes;
using SharedApi.Config;
using SharedApi.Interfaces;
using SharedApi.Services;

namespace SharedApi;

public static class ConfigureServices
{
    public static void AddSharedApi(this IServiceCollection services, IConfiguration configuration)
    {
        var jwtConfig = configuration.GetSection(nameof(JwtConfig));
        var jwtSecret = jwtConfig.GetValue<string>("Secret");
        
        services.Configure<JwtConfig>(jwtConfig);

        services
            .AddAuthentication(AuthConfig.ConfigureAuthOptions)
            .AddJwtBearer(options => JwtBearerConfig.ConfigureJwtBearerOptions(options, jwtSecret));
        
        services.AddSwaggerGen(SwaggerConfig.ConfigureSwaggerOptions);

        services
            .AddControllers(options => options.Filters.Add<ApiExceptionFilterAttribute>())
            .AddFluentValidation(options => options.RegisterValidatorsFromAssembly(Assembly.GetExecutingAssembly()));
        
        services.AddScoped<ICurrentUserService, CurrentUserService>();
        services.AddScoped<IDateTimeService, DateTimeService>();
    }
}