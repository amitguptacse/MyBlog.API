using Microsoft.EntityFrameworkCore;
using MyBlog.API.Data;
using MyBlog.API.DTOs.Categories;
using MyBlog.API.Models;
using Slugify;

namespace MyBlog.API.Services;

public class CategoryService(AppDbContext db) : ICategoryService
{
    private static readonly SlugHelper _slugger = new();

    public async Task<List<CategoryResponseDto>> GetAllAsync() =>
        await db.Categories
            .Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                PostCount = c.BlogPosts.Count(p => p.IsPublished)
            }).ToListAsync();

    public async Task<CategoryResponseDto?> GetByIdAsync(int id) =>
        await db.Categories
            .Where(c => c.Id == id)
            .Select(c => new CategoryResponseDto
            {
                Id = c.Id,
                Name = c.Name,
                Slug = c.Slug,
                PostCount = c.BlogPosts.Count(p => p.IsPublished)
            }).FirstOrDefaultAsync();

    public async Task<CategoryResponseDto> CreateAsync(CategoryDto dto)
    {
        var category = new Category
        {
            Name = dto.Name,
            Slug = _slugger.GenerateSlug(dto.Name)
        };
        db.Categories.Add(category);
        await db.SaveChangesAsync();
        return new CategoryResponseDto { Id = category.Id, Name = category.Name, Slug = category.Slug };
    }

    public async Task<CategoryResponseDto?> UpdateAsync(int id, CategoryDto dto)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null) return null;
        category.Name = dto.Name;
        category.Slug = _slugger.GenerateSlug(dto.Name);
        await db.SaveChangesAsync();
        return new CategoryResponseDto { Id = category.Id, Name = category.Name, Slug = category.Slug };
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var category = await db.Categories.FindAsync(id);
        if (category is null) return false;
        db.Categories.Remove(category);
        await db.SaveChangesAsync();
        return true;
    }
}
