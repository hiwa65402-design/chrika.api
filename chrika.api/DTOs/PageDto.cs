using System.ComponentModel.DataAnnotations;

namespace Chrika.Api.Dtos
{
    // بۆ دروستکردنی پەیجێکی نوێ
    public class CreatePageDto
    {
        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        public string? Description { get; set; }
    }

    // بۆ پیشاندانی زانیاری پەیج
    public class PageDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Description { get; set; }
        public string? ProfilePicture { get; set; }
        public string? CoverPicture { get; set; }
        public string? OwnerUsername { get; set; }
        public DateTime CreatedAt { get; set; }
    }
}
