using Microsoft.EntityFrameworkCore;
using MyBlog.API.Data;
using MyBlog.API.DTOs.Auth;
using MyBlog.API.Helpers;
using MyBlog.API.Models;

namespace MyBlog.API.Services;

public class AuthService(AppDbContext db, JwtHelper jwt, IEmailService email) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("Email already in use.");

        var user = new User
        {
            Username = dto.Username,
            Email = dto.Email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(dto.Password)
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        return BuildResponse(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        return await IssueTokens(user);
    }

    public async Task<AuthResponseDto> RefreshTokenAsync(string refreshToken)
    {
        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.RefreshToken == refreshToken && u.RefreshTokenExpiry > DateTime.UtcNow)
            ?? throw new UnauthorizedAccessException("Invalid or expired refresh token.");

        return await IssueTokens(user);
    }

    public async Task ForgotPasswordAsync(string emailAddress)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == emailAddress);

        // Always return success to prevent email enumeration
        if (user is null) return;

        // Generate a secure reset token (reuse refresh token slot)
        user.RefreshToken = jwt.GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddHours(1);
        await db.SaveChangesAsync();

        var resetLink = $"http://localhost:3000/auth/reset-password?token={Uri.EscapeDataString(user.RefreshToken)}&email={Uri.EscapeDataString(user.Email)}";

        await email.SendAsync(
            toEmail: user.Email,
            toName: user.Username,
            subject: "Reset your MyBlog password",
            htmlBody: $"""
                <h2>Password Reset</h2>
                <p>Hi {user.Username},</p>
                <p>Click the link below to reset your password. This link expires in 1 hour.</p>
                <p><a href="{resetLink}">Reset Password</a></p>
                <p>If you did not request this, ignore this email.</p>
                """);
    }

    public async Task ResetPasswordAsync(string emailAddress, string token, string newPassword)
    {
        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.Email == emailAddress &&
            u.RefreshToken == token &&
            u.RefreshTokenExpiry > DateTime.UtcNow)
            ?? throw new InvalidOperationException("Invalid or expired reset link. Please request a new one.");

        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.RefreshToken = null;
        user.RefreshTokenExpiry = null;
        await db.SaveChangesAsync();
    }

    private async Task<AuthResponseDto> IssueTokens(User user)
    {
        user.RefreshToken = jwt.GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await db.SaveChangesAsync();
        return BuildResponse(user);
    }

    private AuthResponseDto BuildResponse(User user) => new()
    {
        AccessToken = jwt.GenerateAccessToken(user),
        RefreshToken = user.RefreshToken ?? string.Empty,
        Email = user.Email,
        Username = user.Username,
        Role = user.Role
    };
}
