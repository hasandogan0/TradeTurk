using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Infrastructure.Data.Configurations;

public class PortfolioSnapshotConfiguration : IEntityTypeConfiguration<PortfolioSnapshot>
{
    public void Configure(EntityTypeBuilder<PortfolioSnapshot> builder)
    {
        builder.HasKey(s => s.Id);

        builder.Property(s => s.UserId).IsRequired();
        builder.Property(s => s.TotalValue).HasColumnType("decimal(18, 8)");
        builder.Property(s => s.AvailableUSDT).HasColumnType("decimal(18, 8)");
        builder.Property(s => s.AssetValue).HasColumnType("decimal(18, 8)");
        builder.Property(s => s.TotalPnL).HasColumnType("decimal(18, 8)");
        builder.Property(s => s.RowVersion).IsRowVersion();

        builder.HasIndex(s => new { s.UserId, s.CreatedAt });
    }
}
