using System.Linq.Expressions;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Domain.Common;

namespace TRadeTurk.UnitTests.TestDoubles;

internal class InMemoryRepository<T> : IRepository<T> where T : BaseEntity
{
    private readonly List<T> _items = [];

    public InMemoryRepository(params T[] items)
    {
        _items.AddRange(items);
    }

    public IReadOnlyList<T> Items => _items;

    public Task<T?> GetByIdAsync(Guid id, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_items.FirstOrDefault(x => x.Id == id));
    }

    public Task<IEnumerable<T>> GetAllAsync(CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_items.AsEnumerable());
    }

    public Task<IEnumerable<T>> FindAsync(Expression<Func<T, bool>> predicate, CancellationToken cancellationToken = default)
    {
        return Task.FromResult(_items.AsQueryable().Where(predicate).AsEnumerable());
    }

    public Task AddAsync(T entity, CancellationToken cancellationToken = default)
    {
        _items.Add(entity);
        return Task.CompletedTask;
    }

    public void Update(T entity)
    {
    }

    public void Delete(T entity)
    {
        _items.Remove(entity);
    }
}
