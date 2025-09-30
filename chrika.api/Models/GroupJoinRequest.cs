using System;

namespace Chrika.Api.Models
{
    public enum RequestStatus { Pending, Accepted, Rejected }

    public class GroupJoinRequest
    {
        public int Id { get; set; }
        public int GroupId { get; set; }
        public virtual Group? Group { get; set; }

        public int UserId { get; set; }
        public virtual User? User { get; set; }

        public RequestStatus Status { get; set; } = RequestStatus.Pending;
        public DateTime RequestedAt { get; set; } = DateTime.UtcNow;
    }
}


