namespace MsHMI.UserManagement.Core.Interfaces;

/// <summary>
/// Unit of work for managing database transactions.
/// </summary>
public interface IUnitOfWork : IDisposable
{
    /// <summary>
    /// Begin a new transaction.
    /// </summary>
    Task BeginTransactionAsync(CancellationToken ct = default);

    /// <summary>
    /// Commit the current transaction.
    /// </summary>
    Task CommitAsync(CancellationToken ct = default);

    /// <summary>
    /// Rollback the current transaction.
    /// </summary>
    Task RollbackAsync(CancellationToken ct = default);

    /// <summary>
    /// Save all pending changes.
    /// </summary>
    Task<int> SaveChangesAsync(CancellationToken ct = default);
}
