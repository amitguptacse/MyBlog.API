using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBlog.API.DTOs.Posts;
using MyBlog.API.Services;

namespace MyBlog.API.Controllers;

[ApiController]
[Route("api/posts")]
public class PostsController(IPostService postService) : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 10,
        [FromQuery] string? search = null,
        [FromQuery] int? categoryId = null,
        [FromQuery] int? tagId = null,
        [FromQuery] bool includeDrafts = false)
    {
        // Only authenticated users may request drafts
        if (includeDrafts && !User.Identity!.IsAuthenticated)
            includeDrafts = false;

        var (posts, total) = await postService.GetAllAsync(page, pageSize, search, categoryId, tagId, includeDrafts);
        return Ok(new { data = posts, total, page, pageSize });
    }

    [Authorize]
    [HttpGet("id/{id:int}")]
    public async Task<IActionResult> GetById(int id)
    {
        var post = await postService.GetByIdAsync(id);
        return post is null ? NotFound() : Ok(post);
    }

    [HttpGet("{slug}")]
    public async Task<IActionResult> GetBySlug(string slug)
    {
        var post = await postService.GetBySlugAsync(slug);
        return post is null ? NotFound() : Ok(post);
    }

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreatePostDto dto)
    {
        var authorId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var post = await postService.CreateAsync(dto, authorId);
        return CreatedAtAction(nameof(GetBySlug), new { slug = post.Slug }, post);
    }

    [Authorize]
    [HttpPut("{id:int}")]
    public async Task<IActionResult> Update(int id, UpdatePostDto dto)
    {
        var requesterId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var post = await postService.UpdateAsync(id, dto, requesterId, isAdmin);
        return post is null ? NotFound() : Ok(post);
    }

    [Authorize]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var requesterId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var isAdmin = User.IsInRole("Admin");
        var deleted = await postService.DeleteAsync(id, requesterId, isAdmin);
        return deleted ? NoContent() : NotFound();
    }
}
