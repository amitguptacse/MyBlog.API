using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBlog.API.DTOs.Comments;
using MyBlog.API.Services;

namespace MyBlog.API.Controllers;

[ApiController]
[Route("api/comments")]
public class CommentsController(ICommentService commentService) : ControllerBase
{
    [HttpGet("post/{postId:int}")]
    public async Task<IActionResult> GetByPost(int postId) =>
        Ok(await commentService.GetByPostAsync(postId));

    [Authorize(Roles = "Admin")]
    [HttpGet("pending")]
    public async Task<IActionResult> GetPending() =>
        Ok(await commentService.GetPendingAsync());

    [Authorize]
    [HttpPost]
    public async Task<IActionResult> Create(CreateCommentDto dto)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);
        var result = await commentService.CreateAsync(dto, userId);
        return Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpPatch("{id:int}/approve")]
    public async Task<IActionResult> Approve(int id)
    {
        var result = await commentService.ApproveAsync(id);
        return result is null ? NotFound() : Ok(result);
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> Delete(int id)
    {
        var deleted = await commentService.DeleteAsync(id);
        return deleted ? NoContent() : NotFound();
    }
}
