using System.ComponentModel.DataAnnotations;
using System.Collections.Generic;
using System;

namespace Chrika.Api.Models
{
    public class Page
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        public string? ProfilePicture { get; set; }
        public string? CoverPicture { get; set; }

        [StringLength(1000)]
        public string? Bio { get; set; }

        [Phone]
        public string? PhoneNumber { get; set; }

        public string? WhatsAppNumber { get; set; }

        [Url]
        public string? WebsiteUrl { get; set; }

        [StringLength(100)]
        public string? Location { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }

        // Foreign Key to User
        public int OwnerId { get; set; }
        public virtual User? Owner { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // Navigation property
        public virtual ICollection<PagePost> PagePosts { get; set; } = new List<PagePost>();
    }
}
