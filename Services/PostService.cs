using Ganss.Xss;
using Microsoft.EntityFrameworkCore;
using MyBlog.API.Data;
using MyBlog.API.DTOs.Posts;
using MyBlog.API.Models;
using Slugify;

namespace MyBlog.API.Services;

public class PostService(AppDbContext db) : IPostService
{
    private static readonly SlugHelper _slugger = new();
    private static readonly HtmlSanitizer _sanitizer = new();

    public async Task<(List<PostResponseDto> Posts, int Total)> GetAllAsync(int page, int pageSize, string? search, int? categoryId, int? tagId, bool includeDrafts = false)
    {
        var query = db.BlogPosts
            .Where(p => includeDrafts || p.IsPublished)
            .Where(p => search == null
                || p.Title.Contains(search)
                || p.Excerpt!.Contains(search)
                || p.Tags.Any(t => t.Name.Contains(search))
                || p.Content.Contains(search))
            .Where(p => categoryId == null || p.CategoryId == categoryId)
            .Where(p => tagId == null || p.Tags.Any(t => t.Id == tagId))
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .OrderByDescending(p => search != null && p.Title.Contains(search) ? 1 : 0)
            .ThenByDescending(p => p.CreatedAt);

        var total = await query.CountAsync();
        var posts = await query.Skip((page - 1) * pageSize).Take(pageSize).ToListAsync();

        return (posts.Select(Map).ToList(), total);
    }

    public async Task<PostResponseDto?> GetByIdAsync(int id)
    {
        var post = await db.BlogPosts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id);

        return post is null ? null : Map(post);
    }

    public async Task<PostResponseDto?> GetBySlugAsync(string slug)
    {
        var post = await db.BlogPosts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Slug == slug && p.IsPublished);

        return post is null ? null : Map(post);
    }

    private async Task<List<Tag>> ResolveTagsAsync(List<string> tagNames)
    {
        var names = tagNames.Select(t => t.Trim().ToLower()).Where(t => t.Length > 0).Distinct().ToList();
        var existing = await db.Tags.Where(t => names.Contains(t.Name.ToLower())).ToListAsync();
        var existingNames = existing.Select(t => t.Name.ToLower()).ToHashSet();
        foreach (var name in names.Where(n => !existingNames.Contains(n)))
        {
            var tag = new Tag { Name = name, Slug = _slugger.GenerateSlug(name) };
            db.Tags.Add(tag);
            existing.Add(tag);
        }
        return existing;
    }

    public async Task<PostResponseDto> CreateAsync(CreatePostDto dto, int authorId)
    {
        var slug = await UniqueSlug(dto.Title);
        var tags = await ResolveTagsAsync(dto.Tags);

        var post = new BlogPost
        {
            Title = dto.Title,
            Slug = slug,
            Content = _sanitizer.Sanitize(dto.Content),
            Excerpt = dto.Excerpt,
            CoverImageUrl = dto.CoverImageUrl,
            IsPublished = dto.IsPublished,
            CategoryId = dto.CategoryId,
            AuthorId = authorId,
            Tags = tags
        };

        db.BlogPosts.Add(post);
        await db.SaveChangesAsync();

        return Map(await LoadPost(post.Id));
    }

    public async Task<PostResponseDto?> UpdateAsync(int id, UpdatePostDto dto, int requesterId, bool isAdmin)
    {
        var post = await db.BlogPosts.Include(p => p.Tags)
            .FirstOrDefaultAsync(p => p.Id == id);

        if (post is null) return null;

        // Only the author or an admin can update
        if (post.AuthorId != requesterId && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to edit this post.");

        if (dto.Title is not null) { post.Title = dto.Title; post.Slug = await UniqueSlug(dto.Title, id); }
        if (dto.Content is not null) post.Content = _sanitizer.Sanitize(dto.Content);
        if (dto.Excerpt is not null) post.Excerpt = dto.Excerpt;
        if (dto.CoverImageUrl is not null) post.CoverImageUrl = dto.CoverImageUrl;
        if (dto.IsPublished.HasValue) post.IsPublished = dto.IsPublished.Value;
        if (dto.CategoryId.HasValue) post.CategoryId = dto.CategoryId;
        if (dto.Tags is not null)
            post.Tags = await ResolveTagsAsync(dto.Tags);

        post.UpdatedAt = DateTime.UtcNow;
        await db.SaveChangesAsync();

        return Map(await LoadPost(post.Id));
    }

    public async Task<bool> DeleteAsync(int id, int requesterId, bool isAdmin)
    {
        var post = await db.BlogPosts.FirstOrDefaultAsync(p => p.Id == id);
        if (post is null) return false;

        if (post.AuthorId != requesterId && !isAdmin)
            throw new UnauthorizedAccessException("You do not have permission to delete this post.");

        db.BlogPosts.Remove(post);
        await db.SaveChangesAsync();
        return true;
    }

    private async Task<string> UniqueSlug(string title, int? excludeId = null)
    {
        var base_ = _slugger.GenerateSlug(title);
        var slug = base_;
        var counter = 1;
        while (await db.BlogPosts.AnyAsync(p => p.Slug == slug && p.Id != excludeId))
            slug = $"{base_}-{counter++}";
        return slug;
    }

    private async Task<BlogPost> LoadPost(int id) =>
        await db.BlogPosts
            .Include(p => p.Author)
            .Include(p => p.Category)
            .Include(p => p.Tags)
            .FirstAsync(p => p.Id == id);

    private static PostResponseDto Map(BlogPost p) => new()
    {
        Id = p.Id,
        Title = p.Title,
        Slug = p.Slug,
        Content = p.Content,
        Excerpt = p.Excerpt,
        CoverImageUrl = p.CoverImageUrl,
        IsPublished = p.IsPublished,
        CreatedAt = p.CreatedAt,
        UpdatedAt = p.UpdatedAt,
        AuthorId = p.Author?.Id ?? 0,
        AuthorName = p.Author?.Username ?? string.Empty,
        CategoryName = p.Category?.Name,
        Tags = p.Tags.Select(t => t.Name).ToList()
    };
}
