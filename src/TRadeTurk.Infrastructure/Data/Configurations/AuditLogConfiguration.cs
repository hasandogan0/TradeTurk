using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using TRadeTurk.Domain.Entities;

namespace TRadeTurk.Infrastructure.Data.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);
        builder.Property(a => a.Action).HasMaxLength(120).IsRequired();
        builder.Property(a => a.IpAddress).HasMaxLength(80);
        builder.Property(a => a.UserAgent).HasMaxLength(300);
        builder.Property(a => a.RowVersion).IsRowVersion();
        builder.HasIndex(a => new { a.UserId, a.CreatedAt });
    }
}
