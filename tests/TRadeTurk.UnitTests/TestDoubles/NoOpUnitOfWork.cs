using TRadeTurk.Application.Common.Interfaces;

namespace TRadeTurk.UnitTests.TestDoubles;

internal class NoOpUnitOfWork : IUnitOfWork
{
    public int SaveChangesCallCount { get; private set; }

    public Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        SaveChangesCallCount++;
        return Task.FromResult(1);
    }

    public void Dispose()
    {
    }
}
