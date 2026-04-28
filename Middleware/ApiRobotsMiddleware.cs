namespace MyBlog.API.Middleware;

/// <summary>
/// Adds X-Robots-Tag: noindex to all /api/* responses so Googlebot doesn't index raw JSON endpoints.
/// </summary>
public class ApiRobotsMiddleware(RequestDelegate next)
{
    public async Task InvokeAsync(HttpContext context)
    {
        if (context.Request.Path.StartsWithSegments("/api"))
            context.Response.OnStarting(() =>
            {
                context.Response.Headers["X-Robots-Tag"] = "noindex, nofollow";
                return Task.CompletedTask;
            });

        await next(context);
    }
}
