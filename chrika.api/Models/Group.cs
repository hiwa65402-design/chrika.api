using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Chrika.Api.Models
{
    public class Group
    {
        public int Id { get; set; }

        [Required]
        [StringLength(100)]
        public string? Name { get; set; }

        [Required]
        [StringLength(50)]
        public string? Username { get; set; } // ناوی بەکارهێنەری تاک

        [StringLength(250)]
        public string? Description { get; set; }

        [StringLength(1000)]
        public string? Bio { get; set; }

        public string? ProfilePictureUrl { get; set; }
        public string? CoverPictureUrl { get; set; }
        public GroupType Type { get; set; }
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime? ExpiresAt { get; set; }

        // پەیوەندییەکان
        public int OwnerId { get; set; }
        public virtual User? Owner { get; set; }

        public virtual ICollection<GroupMember> Members { get; set; } = new List<GroupMember>();
        public virtual ICollection<GroupFollower> Followers { get; set; } = new List<GroupFollower>();
        public virtual ICollection<GroupPost> Posts { get; set; } = new List<GroupPost>();
        public virtual ICollection<GroupJoinRequest> JoinRequests { get; set; } = new List<GroupJoinRequest>();
        public virtual ICollection<Message> Messages { get; set; } = new List<Message>();

    }
}
