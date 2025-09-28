using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema; // ئەمە زیاد بکە

namespace Chrika.Api.Models // یان Chrika.Api.Entities
{
    public class User
    {
        public int Id { get; set; }

        [Required]
        [StringLength(50)]
        public string Username { get; set; } = string.Empty;

        [Required]
        [EmailAddress]
        [StringLength(100)]
        public string Email { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string FirstName { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string LastName { get; set; } = string.Empty;

        [Required]
        public string PasswordHash { get; set; } = string.Empty;

        public string? ProfilePicture { get; set; }
        public string? Bio { get; set; }
        public DateTime DateOfBirth { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;
        public bool IsVerified { get; set; } = false;

        // Navigation properties for social media features
        public virtual ICollection<Post> Posts { get; set; } = new List<Post>();
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();

        // === ڕاستکراوە ===
        // ئەو پەیوەندیانەی کە من تێیدا 'فۆڵۆوەر'م
        [InverseProperty("Follower")]
        public virtual ICollection<Follow> Followings { get; set; } = new List<Follow>();

        // ئەو پەیوەندیانەی کە من تێیدا 'فۆڵۆو کراوم'
        [InverseProperty("Following")]
        public virtual ICollection<Follow> Followers { get; set; } = new List<Follow>();
    }

    public class Post
    {
        public int Id { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public virtual User? User { get; set; }
        public virtual ICollection<Comment> Comments { get; set; } = new List<Comment>();
        public virtual ICollection<Like> Likes { get; set; } = new List<Like>();
    }

    public class Comment
    {
        public int Id { get; set; }
        public int PostId { get; set; }
        public int UserId { get; set; }
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public bool IsActive { get; set; } = true;

        public virtual Post? Post { get; set; }
        public virtual User? User { get; set; }
    }

    public class Like
    {
        // === ڕاستکراوە: Id لابرا و Composite Key بەکارهێنرا ===
        public int PostId { get; set; }
        public int UserId { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual Post? Post { get; set; }
        public virtual User? User { get; set; }
    }

    public class Follow
    {
        // === ڕاستکراوە: Id لابرا و Composite Key بەکارهێنرا ===
        public int FollowerId { get; set; } // ئەو کەسەی فۆڵۆ دەکات
        public int FollowingId { get; set; } // ئەو کەسەی فۆڵۆ دەکرێت
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        public virtual User? Follower { get; set; }
        public virtual User? Following { get; set; }
    }
}
