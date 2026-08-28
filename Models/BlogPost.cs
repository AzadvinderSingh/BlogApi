using Microsoft.EntityFrameworkCore;

namespace BlogApi.Models
{
    // BlogPost class
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

    // UserCredential class - for login/register requests
    public class UserCredential
    {
        public string Username { get; set; }
        public string Password { get; set; }

        // You can add a constructor if needed, but it's optional
        public UserCredential() { }

        public UserCredential(string username, string password)
        {
            Username = username ?? throw new ArgumentNullException(nameof(username));
            Password = password ?? throw new ArgumentNullException(nameof(password));
        }
    }
}
