using System.ComponentModel.DataAnnotations;
using Chrika.Api.Models; // بۆ بەکارهێنانی GroupType

namespace Chrika.Api.DTOs // یان Dtos
{
    // DTO بۆ وەرگرتنی زانیاری لە کاتی دروستکردنی گرووپی نوێ
    public class CreateGroupDto
    {
        [Required(ErrorMessage = "Group name is required.")]
        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; }

        [Required(ErrorMessage = "Group username is required.")]
        [StringLength(50, MinimumLength = 3)]
        [RegularExpression("^[a-zA-Z0-9_.]*$", ErrorMessage = "Username can only contain letters, numbers, underscores, and periods.")]
        public string? Username { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }

        [Required]
        public GroupType Type { get; set; } = GroupType.Public;
    }

    // DTO بۆ پیشاندانی زانیاری گرووپ
    public class GroupDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Username { get; set; }
        public string? Description { get; set; }
        public string? Bio { get; set; }
        public string? ProfilePictureUrl { get; set; }
        public string? CoverPictureUrl { get; set; }
        public string? Type { get; set; }
        public string? OwnerUsername { get; set; }
        public int MemberCount { get; set; }
        public int FollowerCount { get; set; }
        public DateTime CreatedAt { get; set; }
    }


    // DTO بۆ نوێکردنەوەی زانیاری گرووپ
    public class UpdateGroupDto
    {
        [StringLength(100, MinimumLength = 3)]
        public string? Name { get; set; }

        [StringLength(250)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? Bio { get; set; }

        // بۆ وێنەکان، ئێستا تەنها URL وەردەگرین. دواتر دەتوانین سیستەمی upload دروست بکەین.
        public string? ProfilePictureUrl { get; set; }
        public string? CoverPictureUrl { get; set; }
    }

}
