using BlogApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using System.IO;

var contentRoot = Directory.GetCurrentDirectory();
var browserPath = Path.Combine(contentRoot, "dist", "bookstore", "browser");
var webRootExists = Directory.Exists(browserPath);

var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot,
    WebRootPath = webRootExists ? browserPath : null
});

// Configure JWT authentication
var jwtKey = builder.Configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is missing");
var jwtIssuer = builder.Configuration["Jwt:Issuer"]
    ?? throw new InvalidOperationException("Jwt:Issuer is missing");
var jwtAudience = builder.Configuration["Jwt:Audience"]
    ?? throw new InvalidOperationException("Jwt:Audience is missing");

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
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
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

// Setup CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAzureWebsites", policy =>
        policy.WithOrigins("https://books123-b2fqgpcgcsghb2f8.indiasouthcentral-01.azurewebsites.net")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// Retrieve connection string from environment variables (Azure App Settings)
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured.");
}

// Register DbContext
builder.Services.AddDbContext<BlogDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

// Logging for debugging (optional)
if (builder.Environment.IsDevelopment())
{
    var logger = LoggerFactory.Create(b => b.AddConsole()).CreateLogger("Startup");
    logger.LogInformation("Connection string: {ConnectionString}", connectionString);
}

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

app.UseDefaultFiles();
app.UseStaticFiles();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();
app.UseRouting();
app.UseCors("AllowAzureWebsites");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();
app.MapFallbackToFile("index.html");

app.Run();
