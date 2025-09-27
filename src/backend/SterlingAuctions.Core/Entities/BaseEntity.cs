using System.ComponentModel.DataAnnotations;

namespace SterlingAuctions.Core.Entities;

/// <summary>
/// Base entity class with common properties for all entities
/// </summary>
public abstract class BaseEntity
{
    /// <summary>
    /// Unique identifier for the entity
    /// </summary>
    [Key]
    public Guid Id { get; set; } = Guid.NewGuid();

    /// <summary>
    /// Date and time when the entity was created
    /// </summary>
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Date and time when the entity was last updated
    /// </summary>
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Soft delete flag - indicates if the entity is deleted
    /// </summary>
    public bool IsDeleted { get; set; } = false;

    /// <summary>
    /// Date and time when the entity was deleted (if applicable)
    /// </summary>
    public DateTime? DeletedAt { get; set; }

    /// <summary>
    /// Version number for optimistic concurrency control
    /// </summary>
    [Timestamp]
    public byte[]? Version { get; set; }
}
