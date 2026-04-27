using SendGrid;
using SendGrid.Helpers.Mail;

namespace MyBlog.API.Services;

public class EmailService(IConfiguration config) : IEmailService
{
    public async Task SendAsync(string toEmail, string toName, string subject, string htmlBody)
    {
        var client = new SendGridClient(config["SendGrid:ApiKey"]);
        var from = new EmailAddress(config["SendGrid:FromEmail"], config["SendGrid:FromName"]);
        var to = new EmailAddress(toEmail, toName);
        var msg = MailHelper.CreateSingleEmail(from, to, subject, plainTextContent: null, htmlContent: htmlBody);
        await client.SendEmailAsync(msg);
    }
}
