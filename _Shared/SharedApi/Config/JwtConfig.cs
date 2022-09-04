namespace SharedApi.Config;

public class JwtConfig
{
    public string Secret { get; set; }
    public TimeSpan TTL { get; set; }
}