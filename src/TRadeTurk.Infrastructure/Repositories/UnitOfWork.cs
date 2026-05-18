using Microsoft.EntityFrameworkCore;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Infrastructure.Data;

namespace TRadeTurk.Infrastructure.Repositories;

public class UnitOfWork : IUnitOfWork
{
    private readonly ApplicationDbContext _context;

    public UnitOfWork(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<int> SaveChangesAsync(CancellationToken cancellationToken = default)
    {
        try
        {
            return await _context.SaveChangesAsync(cancellationToken);
        }
        catch (DbUpdateConcurrencyException ex)
        {
            throw new InvalidOperationException("Kayit baska bir islem tarafindan degistirildi. Lutfen tekrar deneyin.", ex);
        }
    }

    public void Dispose()
    {
        _context.Dispose();
        GC.SuppressFinalize(this);
    }
}
