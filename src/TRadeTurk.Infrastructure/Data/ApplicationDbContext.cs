using System.Reflection;
using Microsoft.EntityFrameworkCore;
using TRadeTurk.Application.Common.Interfaces;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public static readonly Guid DemoUserId = Guid.Parse("11111111-1111-1111-1111-111111111111");
    public static readonly Guid DemoWalletId = Guid.Parse("22222222-2222-2222-2222-222222222222");

    private readonly ICurrentUserContext? _currentUserContext;

    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options, ICurrentUserContext? currentUserContext = null) : base(options)
    {
        _currentUserContext = currentUserContext;
    }

    private Guid? CurrentUserId => _currentUserContext?.UserId;

    public DbSet<User> Users => Set<User>();
    public DbSet<Asset> Assets => Set<Asset>();
    public DbSet<Card> Cards => Set<Card>();
    public DbSet<Transaction> Transactions => Set<Transaction>();
    public DbSet<Wallet> Wallets => Set<Wallet>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<PortfolioSnapshot> PortfolioSnapshots => Set<PortfolioSnapshot>();
    public DbSet<RefreshToken> RefreshTokens => Set<RefreshToken>();
    public DbSet<AuditLog> AuditLogs => Set<AuditLog>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.ApplyConfigurationsFromAssembly(Assembly.GetExecutingAssembly());

        foreach (var entityType in modelBuilder.Model.GetEntityTypes()
            .Where(t => typeof(TRadeTurk.Domain.Common.BaseEntity).IsAssignableFrom(t.ClrType)))
        {
            modelBuilder.Entity(entityType.ClrType)
                .Property(nameof(TRadeTurk.Domain.Common.BaseEntity.RowVersion))
                .IsRowVersion();
        }

        modelBuilder.Entity<Wallet>()
            .HasQueryFilter(w => !CurrentUserId.HasValue || w.UserId == CurrentUserId.Value);

        modelBuilder.Entity<Asset>()
            .HasQueryFilter(a => !CurrentUserId.HasValue || a.UserId == CurrentUserId.Value);

        modelBuilder.Entity<Transaction>()
            .HasQueryFilter(t => !CurrentUserId.HasValue || t.UserId == CurrentUserId.Value);

        modelBuilder.Entity<Card>()
            .HasQueryFilter(c => !CurrentUserId.HasValue || c.UserId == CurrentUserId.Value);

        modelBuilder.Entity<Order>()
            .HasQueryFilter(o => !CurrentUserId.HasValue || o.UserId == CurrentUserId.Value);

        modelBuilder.Entity<PortfolioSnapshot>()
            .HasQueryFilter(s => !CurrentUserId.HasValue || s.UserId == CurrentUserId.Value);

    }
}
