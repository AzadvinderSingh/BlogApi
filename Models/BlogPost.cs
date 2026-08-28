using Microsoft.EntityFrameworkCore;

namespace BlogApi.Models
{
    // Existing BlogPost class
    public class BlogPost
    {
        public int Id { get; set; }
        public string Title { get; set; }
        public string Author { get; set; }
        public int Year { get; set; }
    }

    // User class
    public class User
    {
        public int Id { get; set; }
        public string Username { get; set; } 
        public string Password { get; set; }
    }

    // Renamed class for credentials or user info
    public class UserCredential
    {
        public string Title { get; set; }
        public string Author { get; set; }
        public string Username { get; set; }
        public string Password { get; set; }

        public UserCredential(string title, string author, string username, string password)
        {
            Title = title ?? throw new ArgumentNullException(nameof(title));
            Author = author ?? throw new ArgumentNullException(nameof(author));
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Password = password ?? throw new ArgumentNullException(nameof(password));
        }
    }
}