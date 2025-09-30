namespace Chrika.Api.DTOs
{
    public class PaymentInitiationDto
    {
        public int CampaignId { get; set; }
        public string? PaymentGateway { get; set; } // "Switch", "ZainCash", etc.
    }
}
