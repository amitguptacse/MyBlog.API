using System.ComponentModel.DataAnnotations;

namespace MyBlog.API.DTOs.Posts;

public class CreatePostDto
{
    [Required]
    public string Title { get; set; } = string.Empty;

    [Required]
    public string Content { get; set; } = string.Empty;

    public string? Excerpt { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool IsPublished { get; set; } = false;
    public int? CategoryId { get; set; }
    public List<string> Tags { get; set; } = [];
}
