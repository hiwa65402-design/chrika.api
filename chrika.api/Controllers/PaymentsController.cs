using Chrika.Api.Data;
using Chrika.Api.DTOs; // ئەمە دواتر دروستی دەکەین
using Chrika.Api.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using System.Threading.Tasks;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class PaymentsController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public PaymentsController(ApplicationDbContext context)
    {
        _context = context;
    }

    // POST: api/payments/initiate
    [HttpPost("initiate")]
    public async Task<ActionResult> InitiatePayment([FromBody] PaymentInitiationDto request)
    {
        var userId = int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier));
        var campaign = await _context.AdCampaigns
            .Include(c => c.PagePost.Page)
            .FirstOrDefaultAsync(c => c.Id == request.CampaignId);


        // if (campaign == null || campaign.PagePost.Page.OwnerId != userId || campaign.Status != "Draft")
        if (campaign == null || campaign.PagePost.Page.OwnerId != userId || campaign.Status != CampaignStatus.Draft) 
        {
            return BadRequest(new { message = "Invalid campaign for payment." });
        }



        // 1. دروستکردنی مامەڵەیەکی نوێ بە دۆخی "Pending"
        var transaction = new Transaction
        {
            AdCampaignId = campaign.Id,
            Amount = campaign.Budget,
            Currency = campaign.Currency,
            PaymentGateway = request.PaymentGateway, // e.g., "Switch"
            Status = "Pending"
        };
        _context.Transactions.Add(transaction);
        await _context.SaveChangesAsync();

        // 2. لۆجیکی خەیاڵی بۆ دروستکردنی داواکاری لە دەروازەی پارەدان
        // لێرەدا، کاتێک API ـی ڕاستەقینەمان هەبوو، داواکارییەک دەنێرین
        // و URL ـی پارەدان وەردەگرین.
        var paymentUrl = $"https://checkout.dummy-gateway.com/pay?transaction_id={transaction.Id}";

        // 3. گەڕاندنەوەی URL بۆ ڕووکار
        return Ok(new { paymentUrl = paymentUrl, transactionId = transaction.Id });
    }
}
