namespace Chrika.Api.Dtos
{
    public class NotificationDto
    {
        public int Id { get; set; }
        public string? Message { get; set; }
        public string? TriggeredByUsername { get; set; }
        public string? TriggeredByProfilePicture { get; set; }
        public DateTime CreatedAt { get; set; }
        public bool IsRead { get; set; }
        public int? EntityId { get; set; }
    }
}
