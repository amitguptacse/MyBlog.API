using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using MyBlog.API.DTOs.Newsletter;
using MyBlog.API.Services;

namespace MyBlog.API.Controllers;

[ApiController]
[Route("api/newsletter")]
public class NewsletterController(INewsletterService newsletterService) : ControllerBase
{
    [Authorize(Roles = "Admin")]
    [HttpGet("subscribers")]
    public async Task<IActionResult> GetSubscribers()
    {
        var list = await newsletterService.GetAllAsync();
        return Ok(list);
    }

    [HttpPost("subscribe")]
    public async Task<IActionResult> Subscribe(SubscribeDto dto)
    {
        await newsletterService.SubscribeAsync(dto.Email);
        return Ok(new { message = "Subscribed successfully." });
    }

    [HttpPost("unsubscribe")]
    public async Task<IActionResult> Unsubscribe(SubscribeDto dto)
    {
        await newsletterService.UnsubscribeAsync(dto.Email);
        return Ok(new { message = "Unsubscribed successfully." });
    }
}
