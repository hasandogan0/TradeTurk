using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Infrastructure.Data.Configurations;

public class OrderConfiguration : IEntityTypeConfiguration<Order>
{
    public void Configure(EntityTypeBuilder<Order> builder)
    {
        builder.HasKey(o => o.Id);

        builder.Property(o => o.UserId).IsRequired();
        builder.Property(o => o.Symbol).HasMaxLength(20).IsRequired();
        builder.Property(o => o.Quantity).HasColumnType("decimal(18, 8)");
        builder.Property(o => o.Price).HasColumnType("decimal(18, 8)");
        builder.Property(o => o.TriggerPrice).HasColumnType("decimal(18, 8)");
        builder.Property(o => o.FilledQuantity).HasColumnType("decimal(18, 8)");
        builder.Property(o => o.AverageFillPrice).HasColumnType("decimal(18, 8)");
        builder.Property(o => o.Total).HasColumnType("decimal(18, 8)");
        builder.Property(o => o.Side).HasConversion<string>().HasMaxLength(10);
        builder.Property(o => o.Type).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(o => o.RowVersion).IsRowVersion();

        builder.HasIndex(o => new { o.UserId, o.Status, o.CreatedAt });
        builder.HasIndex(o => new { o.Status, o.Symbol });
    }
}
