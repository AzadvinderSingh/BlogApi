using System;

namespace BlogApi.Models
{
    public class BlogPost
    {
        public int Id { get; set; }
        public required string Title { get; set; }
        public required string Author { get; set; }
        public int Year { get; set; }
        public DateTime PublishDate { get; set; } = DateTime.UtcNow;
    }

    public class User
    {
        public int Id { get; set; }
        public required string Username { get; set; }
        public required string Password { get; set; }
    }

    public class UserCredential
    {
        public required string Username { get; set; }
        public required string Password { get; set; }
    }
}
