namespace MyBlog.API.Models;

public class User
{
    public int Id { get; set; }
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public string Role { get; set; } = "User";
    public string? RefreshToken { get; set; }
    public DateTime? RefreshTokenExpiry { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public bool IsEmailVerified { get; set; } = false;
    public string? EmailVerificationOtp { get; set; }
    public DateTime? EmailVerificationOtpExpiry { get; set; }

    public ICollection<BlogPost> BlogPosts { get; set; } = [];
    public ICollection<Comment> Comments { get; set; } = [];
}
