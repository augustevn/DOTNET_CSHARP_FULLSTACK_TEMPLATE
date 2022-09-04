namespace SharedApi.Entities;

public class UserProfile : AuditableEntity
{
    public UserInfo UserInfo { get; set; }
    public Guid UserInfoId { get; set; }
    public string LinkedUserId { get; set; }
    public bool IsEmailConfirmed { get; set; }
}