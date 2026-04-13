using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Infrastructure.Data.Configurations;

public class TransactionConfiguration : IEntityTypeConfiguration<Transaction>
{
    public void Configure(EntityTypeBuilder<Transaction> builder)
    {
        builder.HasKey(t => t.Id);

        builder.Property(t => t.Amount).HasColumnType("decimal(18, 8)");
        builder.Property(t => t.Price).HasColumnType("decimal(18, 4)");
        builder.Property(t => t.Commission).HasColumnType("decimal(18, 4)");
        builder.Property(t => t.Slippage).HasColumnType("decimal(18, 4)");
        
        builder.Property(t => t.Symbol)
            .HasMaxLength(20);
    }
}
