using Microsoft.EntityFrameworkCore;
using MyBlog.API.Data;
using MyBlog.API.DTOs.Comments;
using MyBlog.API.Models;

namespace MyBlog.API.Services;

public class CommentService(AppDbContext db) : ICommentService
{
    public async Task<List<CommentResponseDto>> GetByPostAsync(int postId) =>
        await db.Comments
            .Where(c => c.BlogPostId == postId && c.IsApproved)
            .Include(c => c.User)
            .OrderByDescending(c => c.CreatedAt)
            .Select(c => Map(c))
            .ToListAsync();

    public async Task<List<CommentResponseDto>> GetPendingAsync() =>
        await db.Comments
            .Where(c => !c.IsApproved)
            .Include(c => c.User)
            .OrderBy(c => c.CreatedAt)
            .Select(c => Map(c))
            .ToListAsync();

    public async Task<CommentResponseDto> CreateAsync(CreateCommentDto dto, int userId)
    {
        var comment = new Comment
        {
            Content = dto.Content,
            BlogPostId = dto.BlogPostId,
            UserId = userId
        };
        db.Comments.Add(comment);
        await db.SaveChangesAsync();
        await db.Entry(comment).Reference(c => c.User).LoadAsync();
        return Map(comment);
    }

    public async Task<CommentResponseDto?> ApproveAsync(int id)
    {
        var comment = await db.Comments.Include(c => c.User).FirstOrDefaultAsync(c => c.Id == id);
        if (comment is null) return null;
        comment.IsApproved = true;
        await db.SaveChangesAsync();
        return Map(comment);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var comment = await db.Comments.FindAsync(id);
        if (comment is null) return false;
        db.Comments.Remove(comment);
        await db.SaveChangesAsync();
        return true;
    }

    private static CommentResponseDto Map(Comment c) => new()
    {
        Id = c.Id,
        Content = c.Content,
        IsApproved = c.IsApproved,
        CreatedAt = c.CreatedAt,
        AuthorName = c.User?.Username ?? string.Empty,
        BlogPostId = c.BlogPostId
    };
}
