using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.FileProviders;
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
// Safely reads Azure App Service connection settings or local appsettings.json
var connectionString = builder.Configuration.GetConnectionString("BlogDb") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Register Controllers and OpenAPI/Swagger Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var app = builder.Build();

// 4. Configure HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseHttpsRedirection();

// Serve static assets (JS, CSS, images) from the configured WebRootPath
if (!string.IsNullOrEmpty(app.Environment.WebRootPath))
{
    app.UseStaticFiles();
}

app.UseRouting();
app.UseAuthorization();

// 5. Map API Controllers
app.MapControllers();

// 6. SPA Fallback Routing for Angular / React / Vue
// Ensures browser refreshes on deep routes (e.g., /books/12) serve index.html
if (!string.IsNullOrEmpty(app.Environment.WebRootPath))
{
    app.MapFallbackToFile("index.html");
}

app.Run();
