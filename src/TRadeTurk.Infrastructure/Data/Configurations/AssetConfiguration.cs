using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Infrastructure.Data.Configurations;

public class AssetConfiguration : IEntityTypeConfiguration<Asset>
{
    public void Configure(EntityTypeBuilder<Asset> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Symbol)
            .IsRequired()
            .HasMaxLength(20);

        builder.Property(a => a.Amount).HasColumnType("decimal(18, 8)");
        builder.Property(a => a.AverageCost).HasColumnType("decimal(18, 4)");
    }
}
