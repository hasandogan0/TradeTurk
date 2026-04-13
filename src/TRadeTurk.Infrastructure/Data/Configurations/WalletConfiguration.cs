using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Infrastructure.Data.Configurations;

public class WalletConfiguration : IEntityTypeConfiguration<Wallet>
{
    public void Configure(EntityTypeBuilder<Wallet> builder)
    {
        builder.HasKey(w => w.Id);
        
        builder.Property(w => w.FiatBalance)
            .HasColumnType("decimal(18, 4)")
            .IsRequired();
            
        // One-to-Many ilişkiler
        builder.HasMany(w => w.Assets)
            .WithOne(a => a.Wallet)
            .HasForeignKey(a => a.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
            
        builder.HasMany(w => w.Transactions)
            .WithOne(t => t.Wallet)
            .HasForeignKey(t => t.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
            
        // One-to-One ilişki VirtualCard ile
        builder.HasOne(w => w.VirtualCard)
            .WithOne(c => c.Wallet)
            .HasForeignKey<Card>(c => c.WalletId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
