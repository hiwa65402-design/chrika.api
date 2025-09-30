using System;

namespace Chrika.Api.Models
{
    public class Share
    {
        public int Id { get; set; }

        public int PostId { get; set; }
        public virtual Post? Post { get; set; } // یان PagePost

        public int UserId { get; set; }
        public virtual User? User { get; set; }

        public DateTime SharedAt { get; set; } = DateTime.UtcNow;
    }
}
