using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoveLens.Domain.Identity;

namespace MoveLens.Infrastructure.Persistence.Data.Configurations;

public sealed class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.ToTable("RefreshTokens");

        builder.HasKey(r => r.Id);

        builder.Property(r => r.Id)
            .ValueGeneratedNever();

        builder.Property(r => r.Token)
            .IsRequired()
            .HasMaxLength(256);

        builder.HasIndex(r => r.Token)
            .IsUnique();

        builder.Property(r => r.UserId)
            .IsRequired();

        builder.HasIndex(r => r.UserId);

        builder.Property(r => r.ExpiresOnUtc)
            .IsRequired();

 

        builder.Property(r => r.CreatedAtUtc).IsRequired();
        builder.Property(r => r.LastModifiedUtc).IsRequired();
        builder.Property(r => r.CreatedBy).HasMaxLength(100);
        builder.Property(r => r.LastModifiedBy).HasMaxLength(100);

        builder.Ignore(r => r.DomainEvents);
    }
}