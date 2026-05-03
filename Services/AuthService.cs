using Microsoft.EntityFrameworkCore;
using MyBlog.API.Data;
using MyBlog.API.DTOs.Auth;
using MyBlog.API.Helpers;
using MyBlog.API.Models;

namespace MyBlog.API.Services;

public class AuthService(AppDbContext db, JwtHelper jwt, IEmailService email, IConfiguration config) : IAuthService
{
    public async Task<AuthResponseDto> RegisterAsync(RegisterDto dto)
    {
        if (await db.Users.AnyAsync(u => u.Email == dto.Email))
            throw new InvalidOperationException("Email already in use.");

        var otp = GenerateOtp();
        var user = new User
        {
            Username             = dto.Username,
            Email                = dto.Email,
            PasswordHash         = BCrypt.Net.BCrypt.HashPassword(dto.Password),
            IsEmailVerified      = false,
            EmailVerificationOtp = otp,
            EmailVerificationOtpExpiry = DateTime.UtcNow.AddMinutes(10),
        };

        db.Users.Add(user);
        await db.SaveChangesAsync();

        await email.SendAsync(
            toEmail:  user.Email,
            toName:   user.Username,
            subject:  "Verify your theGyanGaagar email",
            htmlBody: EmailTemplates.EmailVerification(user.Username, otp));

        return new AuthResponseDto
        {
            Id                        = user.Id,
            Email                     = user.Email,
            Username                  = user.Username,
            Role                      = user.Role,
            RequiresEmailVerification = true,
        };
    }

    public async Task<AuthResponseDto> VerifyEmailAsync(VerifyEmailDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
            ?? throw new InvalidOperationException("Account not found.");

        if (user.IsEmailVerified)
            return await IssueTokens(user);

        if (user.EmailVerificationOtp != dto.Otp || user.EmailVerificationOtpExpiry < DateTime.UtcNow)
            throw new InvalidOperationException("Invalid or expired verification code.");

        user.IsEmailVerified            = true;
        user.EmailVerificationOtp       = null;
        user.EmailVerificationOtpExpiry = null;
        await db.SaveChangesAsync();

        return await IssueTokens(user);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto dto)
    {
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == dto.Email)
            ?? throw new UnauthorizedAccessException("Invalid credentials.");

        if (!BCrypt.Net.BCrypt.Verify(dto.Password, user.PasswordHash))
            throw new UnauthorizedAccessException("Invalid credentials.");

        if (!user.IsEmailVerified)
        {
            // Resend a fresh OTP so they can complete verification
            var otp = GenerateOtp();
            user.EmailVerificationOtp       = otp;
            user.EmailVerificationOtpExpiry = DateTime.UtcNow.AddMinutes(10);
            await db.SaveChangesAsync();

            await email.SendAsync(
                toEmail:  user.Email,
                toName:   user.Username,
                subject:  "Verify your theGyanGaagar email",
                htmlBody: EmailTemplates.EmailVerification(user.Username, otp));

            return new AuthResponseDto
            {
                Id                        = user.Id,
                Email                     = user.Email,
                Username                  = user.Username,
                Role                      = user.Role,
                RequiresEmailVerification = true,
            };
        }

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
        if (user is null) return; // prevent email enumeration

        user.RefreshToken       = jwt.GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddHours(1);
        await db.SaveChangesAsync();

        var baseUrl   = config["App:BaseUrl"] ?? throw new InvalidOperationException("App:BaseUrl is not configured.");
        var resetLink = $"{baseUrl}/auth/reset-password?token={Uri.EscapeDataString(user.RefreshToken)}&email={Uri.EscapeDataString(user.Email)}";

        await email.SendAsync(
            toEmail:  user.Email,
            toName:   user.Username,
            subject:  "Reset your theGyanGaagar password",
            htmlBody: EmailTemplates.PasswordReset(user.Username, resetLink));
    }

    public async Task ResetPasswordAsync(string emailAddress, string token, string newPassword)
    {
        var user = await db.Users.FirstOrDefaultAsync(u =>
            u.Email == emailAddress &&
            u.RefreshToken == token &&
            u.RefreshTokenExpiry > DateTime.UtcNow)
            ?? throw new InvalidOperationException("Invalid or expired reset link. Please request a new one.");

        user.PasswordHash       = BCrypt.Net.BCrypt.HashPassword(newPassword);
        user.RefreshToken       = null;
        user.RefreshTokenExpiry = null;
        await db.SaveChangesAsync();
    }

    public async Task<ProfileResponseDto> GetProfileAsync(int userId)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");
        return MapProfile(user);
    }

    public async Task<ProfileResponseDto> UpdateProfileAsync(int userId, UpdateProfileDto dto)
    {
        var user = await db.Users.FindAsync(userId)
            ?? throw new KeyNotFoundException("User not found.");

        if (!string.IsNullOrWhiteSpace(dto.Username))
            user.Username = dto.Username.Trim();

        if (!string.IsNullOrWhiteSpace(dto.Email) && dto.Email != user.Email)
        {
            if (await db.Users.AnyAsync(u => u.Email == dto.Email && u.Id != userId))
                throw new InvalidOperationException("Email already in use.");
            user.Email = dto.Email.Trim();
        }

        if (dto.Bio is not null)
            user.Bio = dto.Bio.Trim();

        if (dto.ProfilePictureUrl is not null)
            user.ProfilePictureUrl = dto.ProfilePictureUrl;

        await db.SaveChangesAsync();
        return MapProfile(user);
    }

    private static ProfileResponseDto MapProfile(User user) => new()
    {
        Id                = user.Id,
        Username          = user.Username,
        Email             = user.Email,
        Bio               = user.Bio,
        ProfilePictureUrl = user.ProfilePictureUrl,
        Role              = user.Role,
        CreatedAt         = user.CreatedAt,
    };

    private async Task<AuthResponseDto> IssueTokens(User user)
    {
        user.RefreshToken       = jwt.GenerateRefreshToken();
        user.RefreshTokenExpiry = DateTime.UtcNow.AddDays(7);
        await db.SaveChangesAsync();
        return BuildResponse(user);
    }

    private AuthResponseDto BuildResponse(User user) => new()
    {
        Id           = user.Id,
        AccessToken  = jwt.GenerateAccessToken(user),
        RefreshToken = user.RefreshToken ?? string.Empty,
        Email        = user.Email,
        Username     = user.Username,
        Role         = user.Role,
    };

    private static string GenerateOtp() =>
        Random.Shared.Next(100_000, 999_999).ToString();
}
