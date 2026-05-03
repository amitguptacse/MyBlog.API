namespace MyBlog.API.Helpers;

public static class EmailTemplates
{
    private const string PrimaryColor  = "#FF5500";
    private const string PrimaryDark   = "#CC4400";
    private const string BgColor       = "#F8F5F0";
    private const string TextColor     = "#1A1A1A";
    private const string MutedColor    = "#666666";

    // ── Base layout ──────────────────────────────────────────────────────────
    private static string Wrap(string content, string? footerExtra = null) => $"""
        <!DOCTYPE html>
        <html lang="en">
        <head>
          <meta charset="UTF-8" />
          <meta name="viewport" content="width=device-width, initial-scale=1.0" />
          <title>theGyanGaagar</title>
        </head>
        <body style="margin:0;padding:40px 20px;background:{BgColor};font-family:'Sora',Arial,Helvetica,sans-serif;">
          <div style="max-width:560px;margin:0 auto;">

            <!-- Header -->
            <div style="background:linear-gradient(135deg,{PrimaryColor} 0%,#FF8C00 100%);border-radius:16px 16px 0 0;padding:32px 40px;text-align:center;">
              <h1 style="margin:0;color:#fff;font-size:26px;font-weight:800;letter-spacing:-0.5px;">theGyanGaagar</h1>
              <p style="margin:6px 0 0;color:rgba(255,255,255,0.85);font-size:13px;font-style:italic;">Gyaan ki roshni, har kadam par.</p>
            </div>

            <!-- Body -->
            <div style="background:#ffffff;padding:40px;border-left:1px solid #EDE8E1;border-right:1px solid #EDE8E1;">
              {content}
            </div>

            <!-- Footer -->
            <div style="background:#EDE8E1;border-radius:0 0 16px 16px;padding:24px 40px;text-align:center;border:1px solid #DDD6CC;border-top:none;">
              <p style="margin:0 0 6px;color:{MutedColor};font-size:12px;">© {DateTime.UtcNow.Year} theGyanGaagar. All rights reserved.</p>
              {footerExtra ?? string.Empty}
            </div>

          </div>
        </body>
        </html>
        """;

    // ── Reusable button ───────────────────────────────────────────────────────
    private static string Button(string url, string label) => $"""
        <div style="text-align:center;margin:32px 0;">
          <a href="{url}"
             style="display:inline-block;background:{PrimaryColor};color:#fff;text-decoration:none;
                    padding:14px 36px;border-radius:10px;font-size:15px;font-weight:700;
                    letter-spacing:0.2px;">
            {label}
          </a>
        </div>
        """;

    // ── Divider ───────────────────────────────────────────────────────────────
    private static string Divider() =>
        """<hr style="border:none;border-top:1px solid #F0EBE3;margin:28px 0;" />""";

    // ── Heading helper ────────────────────────────────────────────────────────
    private static string H2(string text) =>
        $"""<h2 style="margin:0 0 16px;font-size:22px;font-weight:700;color:{TextColor};">{text}</h2>""";

    private static string P(string html, string extraStyle = "") =>
        $"""<p style="margin:0 0 16px;font-size:15px;line-height:1.7;color:{TextColor};{extraStyle}">{html}</p>""";

    private static string Small(string html) =>
        $"""<p style="margin:0;font-size:12px;line-height:1.6;color:{MutedColor};">{html}</p>""";

    // =========================================================================
    // A. Email Verification OTP
    // =========================================================================
    public static string EmailVerification(string username, string otp) => Wrap($"""
        {H2("Verify your email address")}
        {P($"Hi <strong>{username}</strong>, welcome to theGyanGaagar!")}
        {P("Use the one-time code below to verify your email. It expires in <strong>10 minutes</strong>.")}
        <div style="background:#FFF7F2;border:2px dashed {PrimaryColor};border-radius:12px;padding:24px;text-align:center;margin:24px 0;">
          <span style="font-size:40px;font-weight:800;letter-spacing:10px;color:{PrimaryColor};">{otp}</span>
        </div>
        {P("Enter this code on the verification page to activate your account.")}
        {Divider()}
        {Small("If you did not create an account on theGyanGaagar, you can safely ignore this email.")}
        """);

    // =========================================================================
    // B. Password Reset
    // =========================================================================
    public static string PasswordReset(string username, string resetUrl) => Wrap($"""
        {H2("Reset your password")}
        {P($"Hi <strong>{username}</strong>,")}
        {P("We received a request to reset your password. Click the button below to choose a new one. This link expires in <strong>1 hour</strong>.")}
        {Button(resetUrl, "Reset Password")}
        {Divider()}
        {P("Or copy and paste this URL into your browser:")}
        <p style="word-break:break-all;font-size:12px;color:{MutedColor};background:#F8F5F0;padding:12px 16px;border-radius:8px;margin:0 0 16px;">{resetUrl}</p>
        {Divider()}
        {Small("If you did not request a password reset, you can safely ignore this email. Your password will not change.")}
        """);

    // =========================================================================
    // C. Newsletter Welcome / Subscription Confirmation
    // =========================================================================
    public static string NewsletterWelcome(string email, string unsubscribeUrl) => Wrap(
        content: $"""
            {H2("You're in! 🎉")}
            {P("Thanks for subscribing to <strong>theGyanGaagar</strong> newsletter.")}
            {P("You'll be the first to know when we publish new articles, guides, and stories — gyaan straight to your inbox.")}
            <div style="background:#FFF7F2;border-left:4px solid {PrimaryColor};border-radius:0 8px 8px 0;padding:16px 20px;margin:20px 0;">
              {P("Stay curious. Keep learning. Har kadam pe gyaan.", $"margin:0;color:{PrimaryColor};font-weight:600;")}
            </div>
            {Divider()}
            {Small($"You subscribed with <strong>{email}</strong>. <a href=\"{unsubscribeUrl}\" style=\"color:{PrimaryColor};\">Unsubscribe</a> at any time.")}
            """,
        footerExtra: $"""<p style="margin:4px 0 0;font-size:11px;color:{MutedColor};"><a href="{unsubscribeUrl}" style="color:{MutedColor};">Unsubscribe</a></p>""");

    // =========================================================================
    // D. Newsletter Broadcast (post announcement)
    // =========================================================================
    public static string NewsletterBroadcast(string subject, string bodyHtml, string email, string unsubscribeUrl) => Wrap(
        content: $"""
            {H2(subject)}
            {bodyHtml}
            {Divider()}
            {Small($"You're receiving this because you subscribed as <strong>{email}</strong>. <a href=\"{unsubscribeUrl}\" style=\"color:{PrimaryColor};\">Unsubscribe</a>")}
            """,
        footerExtra: $"""<p style="margin:4px 0 0;font-size:11px;color:{MutedColor};"><a href="{unsubscribeUrl}" style="color:{MutedColor};">Unsubscribe</a></p>""");

    // =========================================================================
    // E. Contact Form Notification (to blog owner)
    // =========================================================================
    public static string ContactNotification(string senderName, string senderEmail, string subject, string message) => Wrap($"""
        {H2($"New contact message: {subject}")}
        <table style="width:100%;border-collapse:collapse;margin-bottom:24px;">
          <tr><td style="padding:10px 0;border-bottom:1px solid #F0EBE3;width:100px;color:{MutedColor};font-size:13px;font-weight:600;">From</td>
              <td style="padding:10px 0;border-bottom:1px solid #F0EBE3;font-size:14px;color:{TextColor};">{senderName} &lt;{senderEmail}&gt;</td></tr>
          <tr><td style="padding:10px 0;color:{MutedColor};font-size:13px;font-weight:600;">Subject</td>
              <td style="padding:10px 0;font-size:14px;color:{TextColor};">{subject}</td></tr>
        </table>
        <div style="background:#F8F5F0;border-radius:10px;padding:20px;margin-bottom:8px;">
          <p style="margin:0;font-size:14px;line-height:1.8;color:{TextColor};white-space:pre-wrap;">{System.Net.WebUtility.HtmlEncode(message)}</p>
        </div>
        {Divider()}
        {Small($"Reply directly to <a href=\"mailto:{senderEmail}\" style=\"color:{PrimaryColor};\">{senderEmail}</a>")}
        """);
}
