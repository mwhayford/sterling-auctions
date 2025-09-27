using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace SterlingAuctions.Core.Entities;

/// <summary>
/// Junction entity for auction participants
/// </summary>
public class AuctionParticipant : BaseEntity
{
    /// <summary>
    /// ID of the auction
    /// </summary>
    [Required]
    public Guid AuctionId { get; set; }

    /// <summary>
    /// Navigation property for the auction
    /// </summary>
    public virtual Auction Auction { get; set; } = null!;

    /// <summary>
    /// ID of the participant
    /// </summary>
    [Required]
    public string ParticipantId { get; set; } = string.Empty;

    /// <summary>
    /// Navigation property for the participant
    /// </summary>
    public virtual ApplicationUser Participant { get; set; } = null!;

    /// <summary>
    /// Date when the user registered for the auction
    /// </summary>
    public DateTime RegistrationDate { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Registration fee paid (if any)
    /// </summary>
    [Column(TypeName = "decimal(18,2)")]
    public decimal? RegistrationFeePaid { get; set; }

    /// <summary>
    /// Whether the participant is approved to bid
    /// </summary>
    public bool IsApproved { get; set; } = true;

    /// <summary>
    /// Date when the participant was approved
    /// </summary>
    public DateTime? ApprovalDate { get; set; }

    /// <summary>
    /// ID of the admin who approved the participant
    /// </summary>
    public string? ApprovedById { get; set; }

    /// <summary>
    /// Navigation property for the admin who approved
    /// </summary>
    public virtual ApplicationUser? ApprovedBy { get; set; }

    /// <summary>
    /// Notes about the participant
    /// </summary>
    [MaxLength(1000)]
    public string? Notes { get; set; }
}
