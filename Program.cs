using BlogApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure JWT Authentication
// Supports both 'Jwt:Key' and 'Jwt__Key' (Azure Linux format)
var jwtKey = builder.Configuration["Jwt:Key"] 
    ?? throw new InvalidOperationException("JWT Secret Key 'Jwt:Key' (or 'Jwt__Key') is missing from configuration.");

var jwtIssuer = builder.Configuration["Jwt:Issuer"] 
    ?? "https://books123-b2fqgpcgcsghb2f8.indiasouthcentral-01.azurewebsites.net";

var jwtAudience = builder.Configuration["Jwt:Audience"] 
    ?? "https://books123-b2fqgpcgcsghb2f8.indiasouthcentral-01.azurewebsites.net";

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

// 2. Setup CORS
var allowedOrigins = new[]
{
    "https://books123-b2fqgpcgcsghb2f8.indiasouthcentral-01.azurewebsites.net",
    "http://localhost:4200",
    "https://localhost:7000"
};

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAzureWebsites", policy =>
        policy.WithOrigins(allowedOrigins)
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// 3. Database Setup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection")
    ?? throw new InvalidOperationException("Connection string 'DefaultConnection' is missing from configuration.");

builder.Services.AddDbContext<BlogDbContext>(options =>
{
    options.UseSqlServer(connectionString, sqlOptions =>
    {
        // Enables auto-retry for transient Azure SQL connection hiccups
        sqlOptions.EnableRetryOnFailure(
            maxRetryCount: 5,
            maxRetryDelay: TimeSpan.FromSeconds(30),
            errorNumbersToAdd: null);
    });
});

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();

var app = builder.Build();

// 4. Exception Handling
if (!app.Environment.IsDevelopment())
{
    app.UseExceptionHandler("/error");
    app.UseHsts();
}

app.UseHttpsRedirection();

// 5. Serve Static Files (Frontend SPA Assets)
app.UseDefaultFiles();
app.UseStaticFiles();

// 6. Middleware Execution Pipeline Order
app.UseRouting();

// CORS MUST be applied after UseRouting and before UseAuthentication/UseAuthorization
app.UseCors("AllowAzureWebsites");

app.UseAuthentication();
app.UseAuthorization();

// 7. Route Endpoints
app.MapControllers();

// 8. Single Page Application (SPA) Fallback Route
app.MapFallbackToFile("index.html");

app.Run();
