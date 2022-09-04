namespace Mailing.Config;

public class SmtpConfig
{
    public string Host { get; set; }
    public int Port { get; set; }
    public bool UseSsl { get; set; }
    public string Email { get; set; }
    public string Password { get; set; }
}