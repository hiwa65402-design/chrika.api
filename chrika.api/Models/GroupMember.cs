using System;

namespace Chrika.Api.Models
{
    public class GroupMember
    {
        public int GroupId { get; set; }
        public virtual Group? Group { get; set; }

        public int UserId { get; set; }
        public virtual User? User { get; set; }

        public GroupRole Role { get; set; }
        public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
    }
}
