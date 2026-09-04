using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using BlogApi.Data;
using BlogApi.Models;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using System.IdentityModel.Tokens.Jwt;




namespace BlogApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class BlogPostsController : ControllerBase
    {
        private readonly BlogDbContext _context;

        public BlogPostsController(BlogDbContext context)
        {
            _context = context;
        }

        // GET: api/BlogPosts
        [HttpGet]
        public async Task<IActionResult> GetAll()
        {
            var posts = await _context.BlogPosts.ToListAsync();
            return Ok(posts);
        }


        [HttpGet("{id}")]
        public async Task<IActionResult> Get(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null)
                return NotFound();
            return Ok(post);
        }


        [HttpPost]
        public async Task<IActionResult> Create(BlogPost post)
        {
            _context.BlogPosts.Add(post);
            await _context.SaveChangesAsync();
            return CreatedAtAction(nameof(Get), new { id = post.Id }, post);
        }


        [HttpPut("{id}")]
        public async Task<IActionResult> Update(int id, BlogPost post)
        {
            if (id != post.Id)
                return BadRequest();

            var existingPost = await _context.BlogPosts.FindAsync(id);
            if (existingPost == null)
                return NotFound();
            existingPost.Title = post.Title;
            existingPost.Author = post.Author;
            existingPost.Year = post.Year;

            try
            {
                await _context.SaveChangesAsync();
                return Ok(existingPost);
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!BlogPostExists(id))
                    return NotFound();
                else
                    throw;
            }
        }


        [HttpDelete("{id}")]
        public async Task<IActionResult> Delete(int id)
        {
            var post = await _context.BlogPosts.FindAsync(id);
            if (post == null)
                return NotFound();

            _context.BlogPosts.Remove(post);
            await _context.SaveChangesAsync();

            return NoContent();
        }

        private bool BlogPostExists(int id)
        {
            return _context.BlogPosts.Any(e => e.Id == id);
        }
    }



    [ApiController]
    [Route("api/[controller]")]
    public class AuthController : ControllerBase
    {
        private readonly IConfiguration _configuration;
        private readonly BlogDbContext _context;

        public AuthController(IConfiguration configuration, BlogDbContext context)
        {
            _configuration = configuration;
            _context = context;
        }



        [HttpGet]
        public async Task<IActionResult> GetAllUsers()
        {
            var users = await _context.Users
                .Select(u => new { u.Id, u.Username })
                .ToListAsync();

            return Ok(users);
        }


        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterRequest request)
        {

            if (string.IsNullOrWhiteSpace(request.Username) || string.IsNullOrWhiteSpace(request.Password))
                return BadRequest("Username and password are required.");

            var exists = await _context.Users.AnyAsync(u => u.Username == request.Username);
            if (exists)
                return Conflict("Username already taken.");

            var user = new User
            {
                Username = request.Username,
                Password = request.Password
            };

            _context.Users.Add(user);
            await _context.SaveChangesAsync();

            return Ok(new { user.Id, user.Username });
        }



        [HttpPost("login")]
        public IActionResult Login([FromBody] LoginRequest loginRequest)
        {
            if (string.IsNullOrEmpty(loginRequest.Username) || string.IsNullOrEmpty(loginRequest.Password))
            {
                return Unauthorized();
            }

            var claims = new[]
            {
                new Claim(ClaimTypes.Name, loginRequest.Username),
            };

var jwtKey = _configuration["Jwt:Key"]
    ?? throw new InvalidOperationException("Jwt:Key is not configured.");
var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey));
            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var token = new JwtSecurityToken(
                issuer: _configuration["Jwt:Issuer"],
                audience: _configuration["Jwt:Audience"],
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds);

            var tokenString = new JwtSecurityTokenHandler().WriteToken(token);
            return Ok(new { token = tokenString });
        }
    }

}

public class LoginRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}

public class RegisterRequest
{
    public required string Username { get; set; }
    public required string Password { get; set; }
}
