using MyBlog.API.DTOs.Comments;

namespace MyBlog.API.Services;

public interface ICommentService
{
    Task<List<CommentResponseDto>> GetByPostAsync(int postId);
    Task<List<CommentResponseDto>> GetPendingAsync();
    Task<CommentResponseDto> CreateAsync(CreateCommentDto dto, int userId);
    Task<CommentResponseDto?> ApproveAsync(int id);
    Task<bool> DeleteAsync(int id);
}
