using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBlog.API.DTOs.Categories;
using MyBlog.API.Services;

namespace MyBlog.API.Controllers;

[ApiController]
[Route("api/categories")]
public class CategoriesController(ICategoryService categoryService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll()
    {
        Response.Headers.CacheControl = "public, max-age=300, stale-while-revalidate=3600";
        return Ok(await categoryService.GetAllAsync());
    }

    [HttpGet("{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await categoryService.GetByIdAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPost]
    public async Task<IActionResult> Create(CategoryDto dto)
    {
        var result = await categoryService.CreateAsync(dto);
        return CreatedAtAction(nameof(GetById), new { id = result.Id }, result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, CategoryDto dto)
    {
        var result = await categoryService.UpdateAsync(id, dto);
        return result is null ? NotFound() : Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await categoryService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
