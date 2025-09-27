using SterlingAuctions.Core.Entities;
using System.Linq.Expressions;

namespace SterlingAuctions.Core.Interfaces;

/// <summary>
/// Generic repository interface for data access
/// </summary>
/// <typeparam name="T">Entity type</typeparam>
public interface IRepository<T> where T : BaseEntity
{
    /// <summary>
    /// Get entity by ID
    /// </summary>
    Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get all entities
    /// </summary>
    Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Find entities by predicate
    /// </summary>
    Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Get single entity by predicate
    /// </summary>
    Task<T?> FirstOrDefaultAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add entity
    /// </summary>
    Task<T> AddAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Add multiple entities
    /// </summary>
    Task AddRangeAsync(IEnumerable<T> entities, CancellationToken cancellationToken = default);

    /// <summary>
    /// Update entity
    /// </summary>
    Task UpdateAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete entity by ID
    /// </summary>
    Task DeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Delete entity
    /// </summary>
    Task DeleteAsync(T entity, CancellationToken cancellationToken = default);

    /// <summary>
    /// Soft delete entity
    /// </summary>
    Task SoftDeleteAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Count entities
    /// </summary>
    Task<int> CountAsync(CancellationToken cancellationToken = default);

    /// <summary>
    /// Count entities by predicate
    /// </summary>
    Task<int> CountAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if entity exists
    /// </summary>
    Task<bool> ExistsAsync(Guid id, CancellationToken cancellationToken = default);

    /// <summary>
    /// Check if entity exists by predicate
    /// </summary>
    Task<bool> ExistsAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default);

    /// <summary>
    /// Save changes to database
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
