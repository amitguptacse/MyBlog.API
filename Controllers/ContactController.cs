using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using Microsoft.AspNetCore.Mvc;
using MyBlog.API.Data;
using MyBlog.API.DTOs;
using MyBlog.API.Models;
using MyBlog.API.Services;

namespace MyBlog.API.Controllers;

[ApiController]
[Route("api/contact")]
public class ContactController(AppDbContext db, IEmailService emailService, IConfiguration config) : ControllerBase
{
    [HttpPost]
    public async Task<IActionResult> Send(ContactDto dto)
    {
        // 1. Always save to DB
        var msg = new ContactMessage
        {
            Name    = dto.Name,
            Email   = dto.Email,
            Subject = dto.Subject,
            Message = dto.Message,
        };
        db.ContactMessages.Add(msg);
        await db.SaveChangesAsync();

        // 2. Try to send email — failure is non-fatal
        try
        {
            var ownerEmail = config["Contact:OwnerEmail"] ?? config["SendGrid:FromEmail"];
            if (!string.IsNullOrEmpty(ownerEmail))
            {
                var html = $"""
                    <p><strong>From:</strong> {dto.Name} &lt;{dto.Email}&gt;</p>
                    <p><strong>Subject:</strong> {dto.Subject}</p>
                    <hr/>
                    <p>{dto.Message.Replace("\n", "<br/>")}</p>
                    """;
                await emailService.SendAsync(ownerEmail, "Blog Owner", $"[theGyanGaagar Contact] {dto.Subject}", html);
            }
        }
        catch { /* email is best-effort */ }

        return Ok(new { message = "Message sent." });
    }

    // Admin: list all messages
    [HttpGet]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> GetAll()
    {
        var messages = await db.ContactMessages
            .OrderByDescending(m => m.CreatedAt)
            .Select(m => new
            {
                m.Id, m.Name, m.Email, m.Subject, m.Message, m.IsRead, m.CreatedAt
            })
            .ToListAsync();
        return Ok(messages);
    }

    // Admin: mark as read
    [HttpPatch("{id:int}/read")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> MarkRead(int id)
    {
        var msg = await db.ContactMessages.FindAsync(id);
        if (msg is null) return NotFound();
        msg.IsRead = true;
        await db.SaveChangesAsync();
        return NoContent();
    }

    // Admin: delete
    [HttpDelete("{id:int}")]
    [Authorize(Roles = "Admin")]
    public async Task<IActionResult> Delete(int id)
    {
        var msg = await db.ContactMessages.FindAsync(id);
        if (msg is null) return NotFound();
        db.ContactMessages.Remove(msg);
        await db.SaveChangesAsync();
        return NoContent();
    }
}
