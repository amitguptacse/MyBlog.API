using MyBlog.API.Models;

namespace MyBlog.API.Services;

public interface IMediaService
{
    Task<Media> UploadAsync(IFormFile file, int userId);
    Task<bool> DeleteAsync(int id, int userId);
    Task<List<Media>> GetAllAsync();
}
