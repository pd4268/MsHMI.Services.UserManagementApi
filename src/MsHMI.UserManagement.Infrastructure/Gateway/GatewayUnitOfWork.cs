using MsHMI.UserManagement.Core.Interfaces;

namespace MsHMI.UserManagement.Infrastructure.Gateway;

/// <summary>
/// A no-op unit of work for gateway operations.
/// The gateway handles transactions internally via autocommit, so this is just a pass-through.
/// </summary>
public class GatewayUnitOfWork : IUnitOfWork
{
    public Task BeginTransactionAsync(CancellationToken ct = default)
    {
        // Gateway operations are atomic - no transaction management needed
        return Task.CompletedTask;
    }

    public Task CommitAsync(CancellationToken ct = default)
    {
        // Gateway auto-commits each operation
        return Task.CompletedTask;
    }

    public Task RollbackAsync(CancellationToken ct = default)
    {
        // Gateway has no rollback capability
        return Task.CompletedTask;
    }

    public Task<int> SaveChangesAsync(CancellationToken ct = default)
    {
        // Gateway operations are immediate - nothing to save
        return Task.FromResult(0);
    }

    public void Dispose()
    {
        // Nothing to dispose
    }
}
