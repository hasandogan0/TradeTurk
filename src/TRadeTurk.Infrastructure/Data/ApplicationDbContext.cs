using Microsoft.EntityFrameworkCore;
using TRadeTurk.Domain.Entities;
using System.Reflection;

namespace TRadeTurk.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Wallet> Wallets => Set<Wallet>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);
        
        // Klasördeki IEntityTypeConfiguration sınıflarını otomatik uygular
        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());
    }
}
