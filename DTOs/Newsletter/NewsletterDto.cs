using System.ComponentModel.DataAnnotations;

namespace MyBlog.API.DTOs.Newsletter;

public class SubscribeDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
