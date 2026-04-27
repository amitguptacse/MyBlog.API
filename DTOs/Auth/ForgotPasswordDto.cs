using System.ComponentModel.DataAnnotations;

namespace MyBlog.API.DTOs.Auth;

public class ForgotPasswordDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;
}
