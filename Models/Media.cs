namespace MyBlog.API.Models;

public class Media
{
    public int Id { get; set; }
    public string FileName { get; set; } = string.Empty;
    public string Url { get; set; } = string.Empty;
    public string? PublicId { get; set; }
    public long SizeBytes { get; set; }
    public DateTime UploadedAt { get; set; } = DateTime.UtcNow;

    public int UploadedById { get; set; }
    public User UploadedBy { get; set; } = null!;
}
