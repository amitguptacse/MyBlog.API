using MyBlog.API.DTOs.Posts;

namespace MyBlog.API.Services;

public interface IPostService
{
    Task<(List<PostResponseDto> Posts, int Total)> GetAllAsync(int page, int pageSize, string? search, int? categoryId, int? tagId, bool includeDrafts = false);
    Task<PostResponseDto?> GetBySlugAsync(string slug);
    Task<PostResponseDto?> GetByIdAsync(int id);
    Task<PostResponseDto> CreateAsync(CreatePostDto dto, int authorId);
    Task<PostResponseDto?> UpdateAsync(int id, UpdatePostDto dto, int requesterId, bool isAdmin);
    Task<bool> DeleteAsync(int id, int requesterId, bool isAdmin);
}
