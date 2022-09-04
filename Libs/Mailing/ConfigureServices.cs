using Mailing.Config;
using Mailing.Interfaces;
using Mailing.Services;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace Mailing;

public static class ConfigureServices
{
    public static void AddMailing(this IServiceCollection services, IConfiguration configuration)
    {
        services.Configure<SmtpConfig>(config =>
        {
            configuration.GetSection(nameof(SmtpConfig)).Bind(config);
        });

        services.AddScoped<IEmailService, EmailService>();
    }
}