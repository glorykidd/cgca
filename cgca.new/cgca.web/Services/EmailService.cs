using cgca.web.Models;
using MailKit.Net.Smtp;
using MailKit.Security;
using MimeKit;

namespace cgca.web.Services;

public class EmailService
{
    private readonly IConfiguration _config;
    private readonly ILogger<EmailService> _logger;

    public EmailService(IConfiguration config, ILogger<EmailService> logger)
    {
        _config = config;
        _logger = logger;
    }

    public async Task SendContactAdminNotificationAsync(ContactSubmission submission)
    {
        var adminEmail = _config["Email:AdminNotificationAddress"];
        if (string.IsNullOrWhiteSpace(adminEmail))
            return;

        var subject = $"New Contact Message: {submission.Name}";
        var body = BuildContactAdminHtml(submission);

        await SendAsync(adminEmail, "CGCA Admin", subject, body);
    }

    public async Task SendContactConfirmationAsync(ContactSubmission submission)
    {
        if (string.IsNullOrWhiteSpace(submission.Email))
            return;

        var subject = "Thank you for contacting Cedar Grove Christian Academy";
        var body = BuildContactConfirmationHtml(submission);

        await SendAsync(submission.Email, submission.Name, subject, body);
    }

    public async Task SendSponsorshipAdminNotificationAsync(SponsorshipSubmission submission)
    {
        var adminEmail = _config["Email:AdminNotificationAddress"];
        if (string.IsNullOrWhiteSpace(adminEmail))
            return;

        var subject = $"New Sponsorship Inquiry: {submission.BusinessName} ({submission.SponsorshipTier})";
        var body = BuildSponsorshipAdminHtml(submission);

        await SendAsync(adminEmail, "CGCA Admin", subject, body);
    }

    public async Task SendSponsorshipConfirmationAsync(SponsorshipSubmission submission)
    {
        if (string.IsNullOrWhiteSpace(submission.Email))
            return;

        var subject = "Thank you for your interest in partnering with Cedar Grove Christian Academy";
        var body = BuildSponsorshipConfirmationHtml(submission);

        await SendAsync(submission.Email, submission.ContactName, subject, body);
    }

    private async Task SendAsync(string toAddress, string toName, string subject, string htmlBody)
    {
        var fromAddress = _config["Email:FromAddress"];
        var fromName = _config["Email:FromName"] ?? "Cedar Grove Christian Academy";
        var host = _config["Email:SmtpHost"];
        var port = int.TryParse(_config["Email:SmtpPort"], out var p) ? p : 587;
        var username = _config["Email:Username"];
        var password = _config["Email:Password"];

        if (string.IsNullOrWhiteSpace(fromAddress) || string.IsNullOrWhiteSpace(host) ||
            string.IsNullOrWhiteSpace(username) || string.IsNullOrWhiteSpace(password))
        {
            _logger.LogWarning("Email not configured — skipping send.");
            return;
        }

        var message = new MimeMessage();
        message.From.Add(new MailboxAddress(fromName, fromAddress));
        message.To.Add(new MailboxAddress(toName, toAddress));
        message.Subject = subject;
        message.Body = new TextPart("html") { Text = htmlBody };

        try
        {
            using var client = new SmtpClient();
            await client.ConnectAsync(host, port, SecureSocketOptions.StartTls);
            await client.AuthenticateAsync(username, password);
            await client.SendAsync(message);
            await client.DisconnectAsync(true);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Failed to send email.");
        }
    }

    internal static string BuildContactAdminHtml(ContactSubmission s)
    {
        var phone = string.IsNullOrWhiteSpace(s.Phone) ? "—" : s.Phone;
        var submittedAt = s.SubmittedAt.ToLocalTime().ToString("f");

        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333;">
              <div style="background: #2d5016; padding: 20px; text-align: center;">
                <h1 style="color: #fff; margin: 0; font-size: 20px;">New Contact Message</h1>
              </div>
              <div style="padding: 24px; background: #f9f9f9;">
                <table style="width: 100%; border-collapse: collapse;">
                  <tr><td style="padding: 8px 0; font-weight: bold; width: 140px;">Name</td><td style="padding: 8px 0;">{Encode(s.Name)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold;">Email</td><td style="padding: 8px 0;">{Encode(s.Email)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold;">Phone</td><td style="padding: 8px 0;">{Encode(phone)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold;">Subject</td><td style="padding: 8px 0;">{Encode(s.Subject)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold; vertical-align: top;">Message</td><td style="padding: 8px 0; white-space: pre-wrap;">{Encode(s.Message)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold;">Submitted</td><td style="padding: 8px 0;">{submittedAt}</td></tr>
                </table>
              </div>
              <div style="padding: 16px; text-align: center; background: #eee; font-size: 12px; color: #666;">
                Cedar Grove Christian Academy · <a href="https://www.cedargrovechristianacademy.org/admin/contact/{s.Id}">View in Admin</a>
              </div>
            </body>
            </html>
            """;
    }

    internal static string BuildContactConfirmationHtml(ContactSubmission s)
    {
        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333;">
              <div style="background: #2d5016; padding: 20px; text-align: center;">
                <h1 style="color: #fff; margin: 0; font-size: 20px;">Thank You for Contacting Us!</h1>
              </div>
              <div style="padding: 24px;">
                <p>Dear {Encode(s.Name)},</p>
                <p>Thank you for reaching out to Cedar Grove Christian Academy. We have received your message and will be in touch with you soon.</p>
                <p>If you have any urgent questions, you can also reach us at:</p>
                <ul>
                  <li>Phone: <a href="tel:+15025434101">(502) 543-4101</a></li>
                  <li>Email: <a href="mailto:contactus@cedargrovechristianacademy.org">contactus@cedargrovechristianacademy.org</a></li>
                </ul>
                <p style="margin-top: 32px;">Blessings,<br><strong>Cedar Grove Christian Academy</strong></p>
              </div>
              <div style="padding: 16px; text-align: center; background: #eee; font-size: 12px; color: #666;">
                Cedar Grove Christian Academy · 4900 Cedar Grove Rd, Shepherdsville, KY 40165
              </div>
            </body>
            </html>
            """;
    }

    internal static string BuildSponsorshipAdminHtml(SponsorshipSubmission s)
    {
        var phone = string.IsNullOrWhiteSpace(s.Phone) ? "—" : s.Phone;
        var message = string.IsNullOrWhiteSpace(s.Message) ? "None" : s.Message;
        var submittedAt = s.SubmittedAt.ToLocalTime().ToString("f");

        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333;">
              <div style="background: #2d5016; padding: 20px; text-align: center;">
                <h1 style="color: #fff; margin: 0; font-size: 20px;">New Sponsorship Inquiry</h1>
              </div>
              <div style="padding: 24px; background: #f9f9f9;">
                <table style="width: 100%; border-collapse: collapse;">
                  <tr><td style="padding: 8px 0; font-weight: bold; width: 160px;">Contact Name</td><td style="padding: 8px 0;">{Encode(s.ContactName)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold;">Business Name</td><td style="padding: 8px 0;">{Encode(s.BusinessName)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold;">Email</td><td style="padding: 8px 0;">{Encode(s.Email)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold;">Phone</td><td style="padding: 8px 0;">{Encode(phone)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold;">Tier</td><td style="padding: 8px 0;">{Encode(s.SponsorshipTier)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold; vertical-align: top;">Message</td><td style="padding: 8px 0; white-space: pre-wrap;">{Encode(message)}</td></tr>
                  <tr><td style="padding: 8px 0; font-weight: bold;">Submitted</td><td style="padding: 8px 0;">{submittedAt}</td></tr>
                </table>
              </div>
              <div style="padding: 16px; text-align: center; background: #eee; font-size: 12px; color: #666;">
                Cedar Grove Christian Academy · <a href="https://www.cedargrovechristianacademy.org/admin/sponsorships/{s.Id}">View in Admin</a>
              </div>
            </body>
            </html>
            """;
    }

    internal static string BuildSponsorshipConfirmationHtml(SponsorshipSubmission s)
    {
        return $"""
            <!DOCTYPE html>
            <html>
            <body style="font-family: Arial, sans-serif; max-width: 600px; margin: 0 auto; color: #333;">
              <div style="background: #2d5016; padding: 20px; text-align: center;">
                <h1 style="color: #fff; margin: 0; font-size: 20px;">Thank You for Your Interest in Partnering With Us!</h1>
              </div>
              <div style="padding: 24px;">
                <p>Dear {Encode(s.ContactName)},</p>
                <p>Thank you for your interest in becoming a <strong>{Encode(s.SponsorshipTier)} Sponsor</strong> of Cedar Grove Christian Academy. We are honored by your support of Christ-centered education in Bullitt County.</p>
                <p>A member of our team will be in touch with you shortly to discuss the details of your partnership.</p>
                <p>If you have any questions in the meantime, please contact:</p>
                <ul>
                  <li>Tray Earnhart, Head of School</li>
                  <li>Phone: <a href="tel:+15025434101">(502) 543-4101</a></li>
                  <li>Email: <a href="mailto:contactus@cedargrovechristianacademy.org">contactus@cedargrovechristianacademy.org</a></li>
                </ul>
                <p style="margin-top: 32px;">Gratefully,<br><strong>Cedar Grove Christian Academy</strong></p>
              </div>
              <div style="padding: 16px; text-align: center; background: #eee; font-size: 12px; color: #666;">
                Cedar Grove Christian Academy · 4900 Cedar Grove Rd, Shepherdsville, KY 40165
              </div>
            </body>
            </html>
            """;
    }

    private static string Encode(string? value) =>
        System.Net.WebUtility.HtmlEncode(value ?? "");
}
