using MyBlog.API.DTOs.Categories;

namespace MyBlog.API.Services;

public interface ICategoryService
{
    Task<List<CategoryResponseDto>> GetAllAsync();
    Task<CategoryResponseDto?> GetByIdAsync(int id);
    Task<CategoryResponseDto> CreateAsync(CategoryDto dto);
    Task<CategoryResponseDto?> UpdateAsync(int id, CategoryDto dto);
    Task<bool> DeleteAsync(int id);
}
