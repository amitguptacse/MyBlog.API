using Microsoft.EntityFrameworkCore;
using MyBlog.API.Data;

namespace MyBlog.API.Services;

public class TagService(AppDbContext db) : ITagService
{
    public async Task<List<TagDto>> GetAllAsync() =>
        await db.Tags
            .OrderBy(t => t.Name)
            .Select(t => new TagDto(t.Id, t.Name, t.Slug, t.BlogPosts.Count))
            .ToListAsync();

    public async Task<bool> DeleteAsync(int id)
    {
        var tag = await db.Tags.FirstOrDefaultAsync(t => t.Id == id);
        if (tag is null) return false;
        db.Tags.Remove(tag);
        await db.SaveChangesAsync();
        return true;
    }
}
