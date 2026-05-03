using Microsoft.EntityFrameworkCore;
using MyBlog.API.Data;
using MyBlog.API.Helpers;
using MyBlog.API.Models;

namespace MyBlog.API.Services;

public class NewsletterService(AppDbContext db, IEmailService email, IConfiguration config) : INewsletterService
{
    public async Task SubscribeAsync(string emailAddress)
    {
        var existing = await db.Newsletters.FirstOrDefaultAsync(n => n.Email == emailAddress);
        if (existing is not null)
        {
            existing.IsActive = true;
            await db.SaveChangesAsync();
        }
        else
        {
            db.Newsletters.Add(new Newsletter { Email = emailAddress });
            await db.SaveChangesAsync();
        }

        var baseUrl        = config["App:BaseUrl"] ?? throw new InvalidOperationException("App:BaseUrl is not configured.");
        var unsubscribeUrl = $"{baseUrl}/newsletter/unsubscribe?email={Uri.EscapeDataString(emailAddress)}";

        await email.SendAsync(
            toEmail:  emailAddress,
            toName:   emailAddress,
            subject:  "Welcome to theGyanGaagar newsletter!",
            htmlBody: EmailTemplates.NewsletterWelcome(emailAddress, unsubscribeUrl));
    }

    public async Task UnsubscribeAsync(string emailAddress)
    {
        var existing = await db.Newsletters.FirstOrDefaultAsync(n => n.Email == emailAddress);
        if (existing is null) return;
        existing.IsActive = false;
        await db.SaveChangesAsync();
    }

    public async Task<List<Newsletter>> GetAllAsync() =>
        await db.Newsletters.OrderByDescending(n => n.SubscribedAt).ToListAsync();

    public async Task BroadcastAsync(string subject, string bodyHtml)
    {
        var subscribers = await db.Newsletters
            .Where(n => n.IsActive)
            .ToListAsync();

        var baseUrl = config["App:BaseUrl"] ?? throw new InvalidOperationException("App:BaseUrl is not configured.");

        foreach (var subscriber in subscribers)
        {
            var unsubscribeUrl = $"{baseUrl}/newsletter/unsubscribe?email={Uri.EscapeDataString(subscriber.Email)}";
            var html = EmailTemplates.NewsletterBroadcast(subject, bodyHtml, subscriber.Email, unsubscribeUrl);

            try
            {
                await email.SendAsync(
                    toEmail:  subscriber.Email,
                    toName:   subscriber.Email,
                    subject:  subject,
                    htmlBody: html);
            }
            catch
            {
                // continue sending to other subscribers if one fails
            }
        }
    }
}
