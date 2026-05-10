using System.Text;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using MyBlog.API.Data;
using MyBlog.API.Helpers;
using MyBlog.API.Middleware;
using MyBlog.API.Services;
using Serilog;

Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft.AspNetCore", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/log-.txt",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

var builder = WebApplication.CreateBuilder(args);
builder.Host.UseSerilog();

// Database
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(10),
            errorNumbersToAdd: null)));

// JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

// CORS
builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.WithOrigins(
                  "http://localhost:3000",
                  "https://thegyangaagar.in",
                  "https://www.thegyangaagar.in")
              .AllowAnyHeader()
              .AllowAnyMethod()));

// Response compression (gzip + brotli)
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Rate Limiting
builder.Services.AddRateLimiter(options =>
{
    // Strict limit on auth endpoints: 10 requests per minute per IP
    options.AddFixedWindowLimiter("auth", o =>
    {
        o.PermitLimit = 10;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });

    // General API limit: 100 requests per minute per IP
    options.AddFixedWindowLimiter("general", o =>
    {
        o.PermitLimit = 100;
        o.Window = TimeSpan.FromMinutes(1);
        o.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        o.QueueLimit = 0;
    });

    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests;
});

// Services
builder.Services.AddScoped<JwtHelper>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IPostService, PostService>();
builder.Services.AddScoped<ICategoryService, CategoryService>();
builder.Services.AddScoped<ITagService, TagService>();
builder.Services.AddScoped<ICommentService, CommentService>();
builder.Services.AddScoped<INewsletterService, NewsletterService>();
builder.Services.AddScoped<IEmailService, EmailService>();
builder.Services.AddScoped<IMediaService, MediaService>();

builder.Services.AddControllers();

// Swagger
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "MyBlog API",
        Version = "v1",
        Description = "ASP.NET Core Web API for MyBlog"
    });

    options.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "Bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Enter your JWT token. Example: Bearer {token}"
    });

    options.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Promote the first registered user to Admin if no Admin exists yet
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<MyBlog.API.Data.AppDbContext>();
    try
    {
        await db.Database.EnsureCreatedAsync();
        if (!db.Users.Any(u => u.Role == "Admin"))
        {
            var first = db.Users.OrderBy(u => u.Id).FirstOrDefault();
            if (first is not null) { first.Role = "Admin"; await db.SaveChangesAsync(); }
        }
    }
    catch (Exception ex)
    {
        Console.Error.WriteLine($"Startup DB check failed: {ex.Message}");
    }
}
if (app.Environment.IsDevelopment())
{
    // Swagger UI (opens at root in dev)
    app.UseSwagger();
    app.UseSwaggerUI(options =>
    {
        options.SwaggerEndpoint("/swagger/v1/swagger.json", "MyBlog API v1");
        options.RoutePrefix = string.Empty;
    });
}



app.UseMiddleware<ErrorHandlingMiddleware>();
app.UseMiddleware<ApiRobotsMiddleware>();
app.UseResponseCompression();
app.UseRateLimiter();
app.UseCors();
// HEAD request bhi handle karta hai 10may for uptime robot 
// /health endpoint mein DB ping bhi karo:

app.MapMethods("/health",
    new[] { "GET", "HEAD" },
    async (AppDbContext db) =>
    {
        try
        {
            // DB ko jagaaye rakho
            await db.Database.ExecuteSqlRawAsync("SELECT 1");
            return Results.Ok("OK");
        }
        catch
        {
            return Results.Ok("OK"); // App toh alive hai
        }
    });
app.UseHttpsRedirection();
app.UseAuthentication();
app.UseAuthorization();

// Apply rate limit policies to routes
app.MapControllers();

// Auth endpoints get stricter rate limit
app.MapGroup("api/auth").RequireRateLimiting("auth");

try
{
    Log.Information("Starting MyBlog API");
    app.Run();
}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
