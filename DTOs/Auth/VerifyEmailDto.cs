using System.ComponentModel.DataAnnotations;

namespace MyBlog.API.DTOs.Auth;

public class VerifyEmailDto
{
    [Required, EmailAddress]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Otp { get; set; } = string.Empty;
}
