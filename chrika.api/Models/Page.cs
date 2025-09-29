using System.ComponentModel.DataAnnotations;

namespace Chrika.Api.Models
{
    public class Page
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string Name { get; set; }

        public string? Description { get; set; }
        public string? ProfilePicture { get; set; }
        public string? CoverPicture { get; set; }

        // کێ خاوەنی پەیجەکەیە
        public int OwnerId { get; set; }
        public virtual User Owner { get; set; }

        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        // پەیوەندی: پەیجێک چەندین پۆستی هەیە
        public virtual ICollection<PagePost> PagePosts { get; set; } = new List<PagePost>();
    }
}
