using MailKit.Net.Smtp;
using Mailing.Config;
using Mailing.Interfaces;
using Microsoft.Extensions.Options;
using MimeKit;

namespace Mailing.Services;

public class EmailService : IEmailService
    {
        private readonly SmtpConfig _smtpConfig;
        public EmailService(IOptions<SmtpConfig> smtpConfig)
        {
            _smtpConfig = smtpConfig.Value;
        }
        
        public async Task SendEmail(string subject, string body, string to, string? replyTo = null)
        {
            var email = new MimeMessage {Sender = MailboxAddress.Parse(_smtpConfig.Email)};

            email.From.Add(MailboxAddress.Parse(_smtpConfig.Email));
            
            email.To.Add(MailboxAddress.Parse(to));

            if (!string.IsNullOrEmpty(replyTo))
            {
                // "CC" or "BCC" alone cannot be used since organisations mostly use the "Reply" (not "Reply All") function which replies to Leashr instead of the interested user.
                // "CC" or "BCC" combined with "Reply-To" has little value, causes more confusion.
                email.ReplyTo.Add(MailboxAddress.Parse(replyTo));
                email.ReplyTo.Add(MailboxAddress.Parse(_smtpConfig.Email)); // Just as backup
            };

            email.Subject = subject;

            var builder = new BodyBuilder {HtmlBody = body};

            email.Body = builder.ToMessageBody();
            
            using var client = new SmtpClient();

            await client.ConnectAsync(_smtpConfig.Host, _smtpConfig.Port, _smtpConfig.UseSsl);
            client.AuthenticationMechanisms.Remove("XOAUTH2");
            await client.AuthenticateAsync(_smtpConfig.Email, _smtpConfig.Password);

            await client.SendAsync(email);

            await client.DisconnectAsync(true);
        }
    }