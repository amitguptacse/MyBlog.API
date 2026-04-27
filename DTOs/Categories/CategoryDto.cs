using System.ComponentModel.DataAnnotations;

namespace MyBlog.API.DTOs.Categories;

public class CategoryDto
{
    [Required]
    public string Name { get; set; } = string.Empty;
}

public class CategoryResponseDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Slug { get; set; } = string.Empty;
    public int PostCount { get; set; }
}
