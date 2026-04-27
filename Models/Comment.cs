namespace MyBlog.API.Models;

public class Comment
{
    public int Id { get; set; }
    public string Content { get; set; } = string.Empty;
    public bool IsApproved { get; set; } = false;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public int BlogPostId { get; set; }
    public BlogPost BlogPost { get; set; } = null!;

    public int UserId { get; set; }
    public User User { get; set; } = null!;
}
