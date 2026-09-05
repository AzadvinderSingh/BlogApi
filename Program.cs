using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BlogApi.Data; // Adjust namespace to match your project structure

// 1. Resolve the frontend build directory path (robust for published apps)
var staticPath = Path.Combine(AppContext.BaseDirectory, "dist", "bookstore", "browser");

var webOptions = new WebApplicationOptions
{
    Args = args,
    WebRootPath = Directory.Exists(staticPath) ? staticPath : null
};

var builder = WebApplication.CreateBuilder(webOptions);

// 2. Configure JWT settings (fail fast if missing)
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing from configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer is missing from configuration.");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience is missing from configuration.");

if (Encoding.UTF8.GetByteCount(jwtKey) < 32)
{
    throw new InvalidOperationException("Jwt:Key must be at least 32 bytes (256 bits) for HMAC-SHA256.");
}

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
.AddJwtBearer(options =>
{
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuer = true,
        ValidateAudience = true,
        ValidateLifetime = true,
        ValidateIssuerSigningKey = true,
        ValidIssuer = jwtIssuer,
        ValidAudience = jwtAudience,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
        ClockSkew = TimeSpan.Zero
    };
});

var netlifyCorsPolicy = "AllowNetlifyApp";

// 3. Configure CORS
var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
    ?? new[] { "https://books123-b2fqgpcgcsghb2f8.indiasouthcentral-01.azurewebsites.net" };

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAzureWebsites", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

builder.Services.AddCors(options =>
{
    options.AddPolicy(name: netlifyCorsPolicy, policy =>
    {
        policy.WithOrigins("https://elaborate-paprenjak-eb570a.netlify.app/") // Replace with your exact Netlify domain
              .AllowAnyHeader()
              .AllowAnyMethod();
              ..AllowCredentials();
    });
});

// 4. Configure Database Context
var connectionString = builder.Configuration.GetConnectionString("BlogDb")
    ?? builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'BlogDb' or 'DefaultConnection' is not configured.");

builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseSqlServer(connectionString));

// 5. Register Controllers and Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddProblemDetails();

var app = builder.Build();

// 6. Configure HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseDeveloperExceptionPage();
}
else
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();

if (!string.IsNullOrEmpty(app.Environment.WebRootPath))
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.UseRouting();
app.UseCors("AllowAzureWebsites");

app.UseCors(netlifyCorsPolicy);

// CRITICAL: UseAuthentication MUST be placed before UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

// 7. Map API Controllers
app.MapControllers();
<<<<<<< HEAD

// 8. Error endpoint used by UseExceptionHandler above.
// Logs the real exception server-side always, and only exposes
// the message in the response body when running in Development.
app.Map("/error", (HttpContext context, ILogger<Program> logger, IWebHostEnvironment env) =>
{
    var feature = context.Features.Get<IExceptionHandlerFeature>();
    var exception = feature?.Error;

    if (exception != null)
    {
        logger.LogError(exception, "Unhandled exception occurred while processing {Path}", feature?.Path);
    }

    return Results.Problem(
        title: "An unexpected error occurred.",
        statusCode: StatusCodes.Status500InternalServerError,
        detail: env.IsDevelopment() ? exception?.ToString() : null
    );
});

// 9. SPA Fallback Routing for Angular / React / Vue
if (!string.IsNullOrEmpty(app.Environment.WebRootPath))
{
    app.MapFallbackToFile("index.html");
}
=======
>>>>>>> 0328a3bd76fe5b3e00e34eedb765761b1854787b

// 8. Error endpoint used by UseExceptionHandler above.
// Logs the real exception server-side always, and only exposes
// the message in the response body when running in Development.
app.Map("/error", (HttpContext context, ILogger<Program> logger, IWebHostEnvironment env) =>
{
    var feature = context.Features.Get<IExceptionHandlerFeature>();
    var exception = feature?.Error;

    if (exception != null)
    {
        logger.LogError(exception, "Unhandled exception occurred while processing {Path}", feature?.Path);
    }

    return Results.Problem(
        title: "An unexpected error occurred.",
        statusCode: StatusCodes.Status500InternalServerError,
        detail: env.IsDevelopment() ? exception?.ToString() : null
    );
});

// 9. SPA Fallback Routing for Angular / React / Vue
if (!string.IsNullOrEmpty(app.Environment.WebRootPath))
{
    app.MapFallbackToFile("index.html");
}

app.Run();
