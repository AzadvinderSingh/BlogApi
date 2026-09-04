using System.Text;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using BlogApi.Data; // Adjust namespace to match your project structure

// 1. Resolve and validate the frontend build directory path
var staticPath = Path.Combine(AppContext.BaseDirectory, "dist", "bookstore", "browser");

var webOptions = new WebApplicationOptions
{
    Args = args,
    // Sets WebRootPath safely during initialization if the folder exists
    WebRootPath = Directory.Exists(staticPath) ? staticPath : null
};

var builder = WebApplication.CreateBuilder(webOptions);

// 2. Configure Database Context
var connectionString = builder.Configuration.GetConnectionString("BlogDb") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Configure JWT Authentication Services
var jwtKey = builder.Configuration["Jwt:Key"] 
    ?? throw new InvalidOperationException("JWT Secret Key is missing from configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"];
var jwtAudience = builder.Configuration["Jwt:Audience"];

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

// 4. Register Controllers and Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// 5. Configure HTTP Request Pipeline
app.UseHttpsRedirection();

// Serve static assets (JS, CSS, images) from the configured WebRootPath
if (!string.IsNullOrEmpty(app.Environment.WebRootPath))
{
    app.UseStaticFiles();
}

app.UseRouting();

// CRITICAL: UseAuthentication MUST be placed before UseAuthorization
app.UseAuthentication();
app.UseAuthorization();

// 6. Map API Controllers
app.MapControllers();

// 7. SPA Fallback Routing for Angular / React / Vue
if (!string.IsNullOrEmpty(app.Environment.WebRootPath))
{
    app.MapFallbackToFile("index.html");
}

app.Run();
