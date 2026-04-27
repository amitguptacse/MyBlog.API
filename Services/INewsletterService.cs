using MyBlog.API.Models;

namespace MyBlog.API.Services;

public interface INewsletterService
{
    Task SubscribeAsync(string email);
    Task UnsubscribeAsync(string email);
    Task<List<Newsletter>> GetAllAsync();
}
