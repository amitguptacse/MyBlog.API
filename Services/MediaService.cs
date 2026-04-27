using CloudinaryDotNet;
using CloudinaryDotNet.Actions;
using Microsoft.EntityFrameworkCore;
using MyBlog.API.Data;
using MyBlog.API.Models;

namespace MyBlog.API.Services;

public class MediaService(AppDbContext db, IConfiguration config) : IMediaService
{
    private Cloudinary CreateCloudinary()
    {
        var account = new Account(
            config["Cloudinary:CloudName"],
            config["Cloudinary:ApiKey"],
            config["Cloudinary:ApiSecret"]);
        return new Cloudinary(account) { Api = { Secure = true } };
    }

    public async Task<Media> UploadAsync(IFormFile file, int userId)
    {
        if (file.Length == 0)
            throw new InvalidOperationException("File is empty.");

        var cloudinary = CreateCloudinary();

        await using var stream = file.OpenReadStream();
        var uploadParams = new ImageUploadParams
        {
            File = new FileDescription(file.FileName, stream),
            Folder = "myblog",
            UseFilename = true,
            UniqueFilename = true
        };

        var result = await cloudinary.UploadAsync(uploadParams);

        if (result.Error is not null)
            throw new InvalidOperationException(result.Error.Message);

        var media = new Media
        {
            FileName = file.FileName,
            Url = result.SecureUrl.ToString(),
            PublicId = result.PublicId,
            SizeBytes = file.Length,
            UploadedById = userId
        };

        db.Media.Add(media);
        await db.SaveChangesAsync();
        return media;
    }

    public async Task<bool> DeleteAsync(int id, int userId)
    {
        var media = await db.Media.FindAsync(id);
        if (media is null) return false;

        var cloudinary = CreateCloudinary();
        if (media.PublicId is not null)
            await cloudinary.DestroyAsync(new DeletionParams(media.PublicId));

        db.Media.Remove(media);
        await db.SaveChangesAsync();
        return true;
    }

    public async Task<List<Media>> GetAllAsync() =>
        await db.Media.OrderByDescending(m => m.UploadedAt).ToListAsync();
}
