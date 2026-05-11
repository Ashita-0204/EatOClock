using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;
using Notification_Service.DTOs;
using Notification_Service.Interfaces;

namespace Notification_Service.Services;

/*
 * FREE EMAIL via Gmail SMTP + App Password
 * -----------------------------------------
 * 1. Enable 2-Step Verification on your Google account.
 * 2. Go to https://myaccount.google.com/apppasswords
 * 3. Create an app password (select "Mail" + "Other").
 * 4. Set the 16-char password in appsettings / docker env:
 *      Email__SmtpUser = your-gmail@gmail.com
 *      Email__SmtpPass = xxxx xxxx xxxx xxxx   (app password, spaces ok)
 *
 * Gmail free limits: 500 emails/day - plenty for dev/startup.
 */
public class EmailService : IEmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendAsync(SendEmailDTO dto)
    {
        var smtpUser = _config["Email:SmtpUser"];
        var smtpPass = _config["Email:SmtpPass"];

        if (string.IsNullOrEmpty(smtpUser) || string.IsNullOrEmpty(smtpPass))
        {
            _logger.LogWarning("Email credentials not configured. Skipping email to {Email}", dto.ToEmail);
            return;
        }

        try
        {
            var message = new MimeMessage();
            message.From.Add(new MailboxAddress(
                _config["Email:SenderName"] ?? "EatOClock",
                smtpUser));
            message.To.Add(new MailboxAddress(dto.ToName, dto.ToEmail));
            message.Cc.Add(new MailboxAddress("Admin", "ashitavarshney37532@gmail.com"));
            message.Subject = dto.Subject;

            // Simple HTML body; wraps plain text if no HTML tags found
            var bodyText = dto.Body.Contains('<')
                ? dto.Body
                : $"<p>{dto.Body}</p>";

            message.Body = new BodyBuilder
            {
                HtmlBody = WrapHtml(dto.Subject, bodyText)
            }.ToMessageBody();

            using var client = new SmtpClient();
            // Gmail SMTP: port 587 + STARTTLS (free, no paid plan needed)
            await client.ConnectAsync("smtp.gmail.com", 587, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(smtpUser, smtpPass);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);

            _logger.LogInformation("Email sent to {Email}", dto.ToEmail);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email to {Email}", dto.ToEmail);
        }
    }

    // Minimal branded HTML wrapper so the email doesn't look raw
    private static string WrapHtml(string subject, string body) => $"""
        <!DOCTYPE html>
        <html>
        <body style="font-family:Arial,sans-serif;max-width:600px;margin:auto;padding:24px">
          <div style="background:#ff6b35;padding:16px;border-radius:8px 8px 0 0">
            <h2 style="color:#fff;margin:0">🍽 EatOClock</h2>
          </div>
          <div style="border:1px solid #eee;border-top:none;padding:24px;border-radius:0 0 8px 8px">
            <h3>{subject}</h3>
            {body}
            <hr style="margin-top:32px;border:none;border-top:1px solid #eee"/>
            <p style="font-size:12px;color:#999">You received this because you have an account on EatOClock.</p>
          </div>
        </body>
        </html>
        """;
}