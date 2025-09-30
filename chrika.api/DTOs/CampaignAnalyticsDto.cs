namespace Chrika.Api.DTOs // یان Chrika.Api.DTOs بەپێی پڕۆژەکەت
{
    public class CampaignAnalyticsDto
    {
        public int CampaignId { get; set; }
        public string? PostContent { get; set; }
        public int TotalImpressions { get; set; }
        public int TotalLikes { get; set; }
        public int TotalComments { get; set; }
        public int TotalClicks { get; set; }
    }
}
