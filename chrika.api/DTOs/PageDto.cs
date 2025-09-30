using System.ComponentModel.DataAnnotations;

namespace Chrika.Api.DTOs
{
    //================================================
    // DTOs for Page
    //================================================

    /// <summary>
    /// DTO for creating a new page.
    /// </summary>
    public class CreatePageDto
    {
        [Required(ErrorMessage = "Page name is required.")]
        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

        [StringLength(50)]
        public string? Category { get; set; }
    }

    /// <summary>
    /// DTO for updating an existing page's information.
    /// </summary>
    public class UpdatePageDto
    {
        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; }

        [StringLength(500)]
        public string? Description { get; set; }

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
    }

    /// <summary>
    /// DTO for displaying detailed page information.
    /// </summary>
    public class PageDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ProfilePicture { get; set; }
        public string? CoverPicture { get; set; }
        public string? OwnerUsername { get; set; }
        public DateTime CreatedAt { get; set; }

        // --- New Properties ---
        public string? Bio { get; set; }
        public string? PhoneNumber { get; set; }
        public string? WhatsAppNumber { get; set; }
        public string? WebsiteUrl { get; set; }
        public string? Location { get; set; }
        public string? Category { get; set; }
    }


    //================================================
    // DTOs for PagePost
    //================================================

    /// <summary>
    /// DTO for creating a new post on a page.
    /// </summary>
    public class CreatePagePostDto
    {
        [Required]
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
    }

    /// <summary>
    /// DTO for displaying a page post.
    /// </summary>
    public class PagePostDto
    {
        public int Id { get; set; }
        public string? Content { get; set; }
        public string? ImageUrl { get; set; }
        public DateTime CreatedAt { get; set; }
        public int PageId { get; set; }
        public bool IsSponsored { get; set; }
        public DateTime? SponsoredUntil { get; set; }
        public string? TargetLocation { get; set; }
    }
}
