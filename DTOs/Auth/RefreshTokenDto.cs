using System.ComponentModel.DataAnnotations;

namespace MyBlog.API.DTOs.Auth;

public class RefreshTokenDto
{
    [Required]
    public string RefreshToken { get; set; } = string.Empty;
}
