using Chrika.Api.Models;

public class Post
{
    // --- پرۆپێرتییە بنەڕەتییەکان ---
    public int Id { get; set; }
    public int UserId { get; set; }
    public string Content { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // --- پەیوەندییەکان ---
    public virtual User? User { get; set; }
    public virtual ICollection<Comment>? Comments { get; set; }
    public virtual ICollection<Like>? Likes { get; set; }

    
}
