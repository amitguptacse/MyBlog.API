using System.ComponentModel.DataAnnotations;

namespace MyBlog.API.DTOs.Tags;

public class TagDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class TagResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int PostCount { get; set; }
}
