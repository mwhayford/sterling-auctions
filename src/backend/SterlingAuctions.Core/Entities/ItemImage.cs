using System.ComponentModel.DataAnnotations;

namespace SterlingAuctions.Core.Entities;

/// <summary>
/// Item image entity for storing auction item images
/// </summary>
public class ItemImage : BaseEntity
{
    /// <summary>
    /// Original filename of the uploaded image
    /// </summary>
    [Required]
    [MaxLength(255)]
    public string FileName { get; set; } = string.Empty;

    /// <summary>
    /// URL of the stored image
    /// </summary>
    [Required]
    [MaxLength(500)]
    public string ImageUrl { get; set; } = string.Empty;

    /// <summary>
    /// URL of the thumbnail image
    /// </summary>
    [MaxLength(500)]
    public string? ThumbnailUrl { get; set; }

    /// <summary>
    /// URL of the medium-sized image
    /// </summary>
    [MaxLength(500)]
    public string? MediumUrl { get; set; }

    /// <summary>
    /// Alt text for accessibility
    /// </summary>
    [MaxLength(255)]
    public string? AltText { get; set; }

    /// <summary>
    /// Image caption
    /// </summary>
    [MaxLength(500)]
    public string? Caption { get; set; }

    /// <summary>
    /// Display order for sorting images
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Whether this is the primary/main image
    /// </summary>
    public bool IsPrimary { get; set; } = false;

    /// <summary>
    /// Image width in pixels
    /// </summary>
    public int? Width { get; set; }

    /// <summary>
    /// Image height in pixels
    /// </summary>
    public int? Height { get; set; }

    /// <summary>
    /// File size in bytes
    /// </summary>
    public long? FileSize { get; set; }

    /// <summary>
    /// MIME type of the image
    /// </summary>
    [MaxLength(100)]
    public string? MimeType { get; set; }

    /// <summary>
    /// ID of the auction item this image belongs to
    /// </summary>
    [Required]
    public Guid AuctionItemId { get; set; }

    /// <summary>
    /// Navigation property for the auction item
    /// </summary>
    public virtual AuctionItem AuctionItem { get; set; } = null!;
}
