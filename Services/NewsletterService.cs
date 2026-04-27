using Microsoft.EntityFrameworkCore;
using MyBlog.API.Data;
using MyBlog.API.Models;

namespace MyBlog.API.Services;

public class NewsletterService(AppDbContext db) : INewsletterService
{
    public async Task SubscribeAsync(string email)
    {
        var existing = await db.Newsletters.FirstOrDefaultAsync(n => n.Email == email);
        if (existing is not null)
        {
            existing.IsActive = true;
            await db.SaveChangesAsync();
            return;
        }
        db.Newsletters.Add(new Newsletter { Email = email });
        await db.SaveChangesAsync();
    }

    public async Task<List<Newsletter>> GetAllAsync() =>
        await db.Newsletters.OrderByDescending(n => n.SubscribedAt).ToListAsync();

    public async Task UnsubscribeAsync(string email)
    {
        var existing = await db.Newsletters.FirstOrDefaultAsync(n => n.Email == email);
        if (existing is null) return;
        existing.IsActive = false;
        await db.SaveChangesAsync();
    }
}
