using Microsoft.AspNetCore.Mvc;
using MyBlog.API.DTOs;
using MyBlog.API.Services;

namespace MyBlog.API.Controllers;

[ApiController]
[Route("api/contact")]
public class ContactController(IEmailService emailService, IConfiguration config) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Send(ContactDto dto)
    {
        var ownerEmail = config["Contact:OwnerEmail"] ?? config["SendGrid:FromEmail"] ?? string.Empty;
        if (string.IsNullOrEmpty(ownerEmail))
            return StatusCode(503, new { message = "Contact email not configured." });

        var html = $"""
            <p><strong>From:</strong> {dto.Name} &lt;{dto.Email}&gt;</p>
            <p><strong>Subject:</strong> {dto.Subject}</p>
            <hr/>
            <p>{dto.Message.Replace("\n", "<br/>")}</p>
            """;

        await emailService.SendAsync(ownerEmail, "Blog Owner", $"[devJournal Contact] {dto.Subject}", html);
        return Ok(new { message = "Message sent." });
    }
}
