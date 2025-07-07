using System;
using System.ComponentModel.DataAnnotations;

namespace PersonalBlog.Models
{
    public class Post
    {
        public int Id { get; set; }

        [Required]
        public string? Title { get; set; }

        [Required]
        public string? Content { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.Now;

        public string? ImageUrl { get; set; }
        public string? Category { get; set; }
        public string? Author { get; set; }

        public ICollection<Comment>? Comments { get; set; }
    }
}