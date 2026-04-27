using System.ComponentModel.DataAnnotations;

namespace MyBlog.API.DTOs.Comments;

public class CreateCommentDto
{
    [Required]
    public string Content { get; set; } = string.Empty;

    [Required]
    public int BlogPostId { get; set; }
}

public class CommentResponseDto
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsApproved { get; set; }
    public DateTime CreatedAt { get; set; }
    public string AuthorName { get; set; } = string.Empty;
    public int BlogPostId { get; set; }
}
