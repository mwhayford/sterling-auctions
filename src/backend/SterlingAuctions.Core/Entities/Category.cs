using System.ComponentModel.DataAnnotations;

namespace SterlingAuctions.Core.Entities;

/// <summary>
/// Category entity for organizing auction items
/// </summary>
public class Category : BaseEntity
{
    /// <summary>
    /// Category name
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Category description
    /// </summary>
    [MaxLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Category icon/image URL
    /// </summary>
    [MaxLength(500)]
    public string? IconUrl { get; set; }

    /// <summary>
    /// Category slug for URL-friendly names
    /// </summary>
    [Required]
    [MaxLength(100)]
    public string Slug { get; set; } = string.Empty;

    /// <summary>
    /// Parent category ID for hierarchical categories
    /// </summary>
    public Guid? ParentCategoryId { get; set; }

    /// <summary>
    /// Navigation property for parent category
    /// </summary>
    public virtual Category? ParentCategory { get; set; }

    /// <summary>
    /// Navigation property for child categories
    /// </summary>
    public virtual ICollection<Category> ChildCategories { get; set; } = new List<Category>();

    /// <summary>
    /// Navigation property for auction items in this category
    /// </summary>
    public virtual ICollection<AuctionItem> AuctionItems { get; set; } = new List<AuctionItem>();

    /// <summary>
    /// Display order for sorting
    /// </summary>
    public int DisplayOrder { get; set; } = 0;

    /// <summary>
    /// Whether the category is active
    /// </summary>
    public bool IsActive { get; set; } = true;
}
