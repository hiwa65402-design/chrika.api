using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;

namespace Chrika.Api.Models
{
    //================================================
    // Enums for Analytics and Ads
    //================================================

    /// <summary>
    /// The type of interaction a user has with an ad.
    /// </summary>
    public enum InteractionType
    {
        Impression, // Ad was shown to the user
        Click,      // User clicked on the ad
        Like,       // User liked the sponsored post
        Comment,    // User commented on the sponsored post
        Share       // User shared the sponsored post
    }

    /// <summary>
    /// The current status of an advertising campaign.
    /// </summary>
    public enum CampaignStatus
    {
        Draft,      // Campaign is being created, not active yet.
        Active,     // Campaign is currently running.
        Paused,     // Campaign is temporarily stopped.
        Completed,  // Campaign has finished its run.
        Archived    // Campaign is old and stored away.
    }


    //================================================
    // Main Models for Analytics and Ads
    //================================================

    /// <summary>
    /// Represents a single interaction with an ad campaign.
    /// </summary>
    public class AdInteraction
    {
        public int Id { get; set; }

        public int AdCampaignId { get; set; }
        public virtual AdCampaign? AdCampaign { get; set; }

        // The user who interacted, can be null if the user was not logged in.
        public int? InteractingUserId { get; set; }
        public virtual User? InteractingUser { get; set; }

        public InteractionType Type { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
    }
}
