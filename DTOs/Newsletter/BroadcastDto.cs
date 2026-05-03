using System.ComponentModel.DataAnnotations;

namespace MyBlog.API.DTOs.Newsletter;

public class BroadcastDto
{
    [Required]
    public string Subject { get; set; } = string.Empty;

    [Required]
    public string BodyHtml { get; set; } = string.Empty;
}
