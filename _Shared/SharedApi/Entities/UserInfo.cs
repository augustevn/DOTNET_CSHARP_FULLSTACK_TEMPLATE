namespace SharedApi.Entities;

public class UserInfo : AuditableEntity
{
    public string Name { get; set; }
    public string Email { get; set; }
    public string? PhoneNumber { get; set; }
}