using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Infrastructure.Data.Configurations;

public class CardConfiguration : IEntityTypeConfiguration<Card>
{
    public void Configure(EntityTypeBuilder<Card> builder)
    {
        builder.HasKey(c => c.Id);

        builder.Property(c => c.UserId)
            .IsRequired();

        builder.Property(c => c.CardHolderName).HasMaxLength(100).IsRequired();
        builder.Property(c => c.CardNumber).HasMaxLength(16).IsRequired();
        builder.Property(c => c.ExpiryMonth).IsRequired();
        builder.Property(c => c.ExpiryYear).IsRequired();
        builder.Property(c => c.CvvHash).HasMaxLength(256).IsRequired();
        
        builder.Property(c => c.Balance).HasColumnType("decimal(18, 4)");
    }
}
