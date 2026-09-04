using BlogApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
using Microsoft.IdentityModel.Tokens;
using System.Text;

// 1. Resolve content root & check browser path prior to builder creation
var contentRoot = Directory.GetCurrentDirectory();
var browserPath = Path.Combine(contentRoot, "dist", "bookstore", "browser");
var hasCustomWebRoot = Directory.Exists(browserPath);

// 2. Pass WebRootPath into WebApplicationOptions BEFORE initialization
var builder = WebApplication.CreateBuilder(new WebApplicationOptions
{
    Args = args,
    ContentRootPath = contentRoot,
    WebRootPath = hasCustomWebRoot ? browserPath : null
});

// 3. Configure JWT Authentication (with fallback to avoid startup crash if env vars are delayed)
var jwtKey = builder.Configuration["Jwt:Key"] ?? "TemporaryFallbackSecretKey1234567890!";
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "https://localhost";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "https://localhost";

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

// 4. Setup CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAzureWebsites", policy =>
        policy.WithOrigins("https://books123-b2fqgpcgcsghb2f8.indiasouthcentral-01.azurewebsites.net")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// 5. Database Connection String Setup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection") 
    ?? builder.Configuration["ConnectionStrings:DefaultConnection"];

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is not configured in Azure Environment Variables.");
}

builder.Services.AddDbContext<BlogDbContext>(options =>
{
    options.UseSqlServer(connectionString);
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// 6. Serve static files from custom SPA directory or default wwwroot
if (hasCustomWebRoot)
{
    var fileProvider = new PhysicalFileProvider(browserPath);
    
    app.UseDefaultFiles(new DefaultFilesOptions
    {
        FileProvider = fileProvider
    });

    app.UseStaticFiles(new StaticFileOptions
    {
        FileProvider = fileProvider
    });
}
else
{
    app.UseDefaultFiles();
    app.UseStaticFiles();
}

app.UseRouting();
app.UseCors("AllowAzureWebsites");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 7. Map fallback for Single Page Application routing
if (hasCustomWebRoot)
{
    app.MapFallbackToFile("index.html", new StaticFileOptions
    {
        FileProvider = new PhysicalFileProvider(browserPath)
    });
}
else
{
    app.MapFallbackToFile("index.html");
}

app.Run();
