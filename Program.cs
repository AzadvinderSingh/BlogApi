using Microsoft.EntityFrameworkCore;
using BlogApi.Data; // Adjust namespace to match your project structure

// 1. Configure WebApplicationOptions before building the app
var webOptions = new WebApplicationOptions
{
    Args = args,
    // Safely sets the static file path without calling builder.WebHost.UseWebRoot()
    WebRootPath = Path.Combine(Directory.GetCurrentDirectory(), "dist", "bookstore", "browser")
};

var builder = WebApplication.CreateBuilder(webOptions);

// 2. Configure Database Context
// In Azure, ConnectionStrings:BlogDb (or ConnectionStrings:DefaultConnection) 
// overrides appsettings.json when configured in App Service Settings.
var connectionString = builder.Configuration.GetConnectionString("BlogDb") 
    ?? builder.Configuration.GetConnectionString("DefaultConnection");

builder.Services.AddDbContext<BlogDbContext>(options =>
    options.UseSqlServer(connectionString));

// 3. Register Controllers and Services
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// 4. Configure HTTP Request Pipeline
if (app.Environment.IsDevelopment())
{
 
}

app.UseHttpsRedirection();

// Enables serving static files (e.g., Angular/React assets) from the custom web root
app.UseStaticFiles();

app.UseAuthorization();

app.MapControllers();

app.Run();
