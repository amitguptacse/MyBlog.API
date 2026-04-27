namespace MyBlog.API.DTOs.Posts;

public class UpdatePostDto
{
    public string? Title { get; set; }
    public string? Content { get; set; }
    public string? Excerpt { get; set; }
    public string? CoverImageUrl { get; set; }
    public bool? IsPublished { get; set; }
    public int? CategoryId { get; set; }
    public List<string>? Tags { get; set; }
}
