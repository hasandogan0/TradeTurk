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

        modelBuilder.Entity<User>().HasData(new
        {
            Id = DemoUserId,
            UserName = "Demo User",
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = (DateTime?)null
        });

        modelBuilder.Entity<Wallet>().HasData(new
        {
            Id = DemoWalletId,
            UserId = DemoUserId,
            FiatBalance = 50000m,
            CreatedAt = new DateTime(2026, 1, 1, 0, 0, 0, DateTimeKind.Utc),
            UpdatedAt = (DateTime?)null
        });
    }
}
