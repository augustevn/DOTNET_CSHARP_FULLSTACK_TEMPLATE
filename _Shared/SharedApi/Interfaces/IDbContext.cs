using Microsoft.EntityFrameworkCore;
using SharedApi.Entities;

namespace SharedApi.Interfaces;

public interface IDbContext
{
    DbSet<UserProfile> UserProfiles { get; set; }
    DbSet<UserInfo> UserInfos { get; set; }
    DbSet<RefreshToken> RefreshTokens { get; set; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken);
}