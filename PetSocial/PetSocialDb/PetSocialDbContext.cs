using System.Reflection;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using SharedApi.Entities;
using SharedApi.Interfaces;

namespace PetSocialDb;

public class PetSocialDbContext : IdentityDbContext<IdentityUser>, IDbContext
{
    public DbSet<UserProfile> UserProfiles { get; set; }
    public DbSet<UserInfo> UserInfos { get; set; }
    public DbSet<RefreshToken> RefreshTokens { get; set; }

    private readonly ICurrentUserService _currentUserService;
    private readonly IDateTimeService _dateTimeService;
    public PetSocialDbContext(DbContextOptions options, ICurrentUserService currentUserService, IDateTimeService dateTimeService) : base(options)
    {
        _currentUserService = currentUserService;
        _dateTimeService = dateTimeService;
    }
    
    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // TODO: Constraints
        // modelBuilder.Entity<UserProfile>().HasIndex(u => u.Email).IsUnique();
        
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
        base.OnModelCreating(modelBuilder);
    }
    
    public override async Task<int> SaveChangesAsync(CancellationToken cancellationToken = new())
    {
        foreach (var entry in ChangeTracker.Entries<AuditableEntity>())
        {
            switch (entry.State)
            {
                case EntityState.Added:
                    entry.Entity.CreatedBy = _currentUserService?.UserId ?? "SYSTEM";
                    entry.Entity.CreatedAt = _dateTimeService.Now;
                    break;
    
                case EntityState.Modified:
                    entry.Entity.LastModifiedBy = _currentUserService?.UserId ?? "SYSTEM";
                    entry.Entity.LastModifiedAt = _dateTimeService.Now;
                    break;
            }
        }
    
        var result = await base.SaveChangesAsync(cancellationToken);
        return result;
    }
}