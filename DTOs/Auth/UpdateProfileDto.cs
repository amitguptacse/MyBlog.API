namespace MyBlog.API.DTOs.Auth;

public class UpdateProfileDto
{
    public string? Username { get; set; }
    public string? Email { get; set; }
    public string? Bio { get; set; }
    public string? ProfilePictureUrl { get; set; }
}
