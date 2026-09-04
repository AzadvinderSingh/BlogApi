using BlogApi.Data;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using System.Text;

var builder = WebApplication.CreateBuilder(args);

// 1. Configure JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] 
    ?? throw new InvalidOperationException("JWT Secret Key 'Jwt:Key' is missing from configuration.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] ?? "https://books123-b2fqgpcgcsghb2f8.indiasouthcentral-01.azurewebsites.net";
var jwtAudience = builder.Configuration["Jwt:Audience"] ?? "https://books123-b2fqgpcgcsghb2f8.indiasouthcentral-01.azurewebsites.net";

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
var azureOrigin = "https://books123-b2fqgpcgcsghb2f8.indiasouthcentral-01.azurewebsites.net";

builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAzureWebsites", policy =>
        policy.WithOrigins(azureOrigin, "http://localhost:4200", "https://localhost:7000")
              .AllowAnyHeader()
              .AllowAnyMethod()
              .AllowCredentials());
});

// 3. Database Setup
var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrEmpty(connectionString))
{
    throw new InvalidOperationException("Connection string 'DefaultConnection' is missing from configuration.");
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

// 4. Standard Static Files Setup (Serves directly from wwwroot)
app.UseDefaultFiles();
app.UseStaticFiles();

// 5. Middleware Pipeline
app.UseRouting();
app.UseCors("AllowAzureWebsites");
app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// 6. SPA Routing Fallback
app.MapFallbackToFile("index.html");

app.Run();
