using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using SharedApi.Config;
using SharedApi.Interfaces;

namespace PetSocialDb;

public static class ConfigureServices
{
    public static void AddPetSocialDb(this IServiceCollection services, IConfiguration configuration)
    {
        services.AddDbContext<PetSocialDbContext>(options => 
            options.UseSqlServer(
                configuration.GetConnectionString("DefaultConnection"), 
                b => b.MigrationsAssembly(typeof(PetSocialDbContext).Assembly.FullName)));
        
        services.AddScoped<IDbContext>(provider => provider.GetService<PetSocialDbContext>());

        services
            .AddDefaultIdentity<IdentityUser>(IdentityConfig.ConfigureIdentityOptions)
            .AddRoles<IdentityRole>()
            .AddEntityFrameworkStores<PetSocialDbContext>()
            .AddDefaultTokenProviders();
    }
}