using System;

namespace Chrika.Api.Models
{
    public class GroupFollower
    {
        public int GroupId { get; set; }
        public virtual Group? Group { get; set; }

        public int UserId { get; set; }
        public virtual User? User { get; set; }

        public DateTime FollowedAt { get; set; } = DateTime.UtcNow;
    }
}
