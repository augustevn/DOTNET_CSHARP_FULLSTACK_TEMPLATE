namespace SharedApi.Entities;

public class RefreshToken : AuditableEntity
{
    public string JwtId { get; set; }
    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; }
    public bool IsInvalidated { get; set; }
    public string LinkedUserId { get; set; }
}