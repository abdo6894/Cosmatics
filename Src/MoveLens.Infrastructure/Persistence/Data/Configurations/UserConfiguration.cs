using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using MoveLens.Domain.Users.ValueObjects;
using MoveLens.Domain.Users.Enums;
using MoveLens.Domain.Users.Entities;

namespace MoveLens.Infrastructure.Persistence.Data.Configurations;

public sealed class UserConfiguration : IEntityTypeConfiguration<User>
{
    public void Configure(EntityTypeBuilder<User> builder)
    {
        builder.ToTable("Users");

        builder.HasKey(u => u.Id);

        builder.Property(u => u.Id)
            .ValueGeneratedNever();

        builder.Property(u => u.FullName)
            .IsRequired()
            .HasMaxLength(100);

        builder.Property(u => u.IdentityId)
            .IsRequired();

        builder.HasIndex(u => u.IdentityId)
            .IsUnique();

        builder.Property(u => u.IsActive)
            .IsRequired()
            .HasDefaultValue(true);

 
        builder.OwnsOne(u => u.Preferences, pref =>
        {
            pref.Property(p => p.Language)
                .HasConversion<string>()
                .HasMaxLength(20);

            pref.Property(p => p.MaxBudget)
                .HasColumnType("decimal(10,2)");

            pref.Property(p => p.PreferredMoods)
                .HasConversion(
                    moods => string.Join(",", moods.Select(m => (int)m)),
                    value => (IReadOnlyList<OutingMood>)value
                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .Select(v => (OutingMood)int.Parse(v))
                        .ToList())
                .HasColumnName("PreferredMoods")
                .HasMaxLength(100);

            pref.Property(p => p.FavoriteGovernorates)
                .HasConversion(
                    govs => string.Join(",", govs),
                    value => (IReadOnlyList<string>)value
                        .Split(",", StringSplitOptions.RemoveEmptyEntries)
                        .ToList())
                .HasColumnName("FavoriteGovernorates")
                .HasMaxLength(500);
        });


        builder.Property(u => u.CreatedAtUtc).IsRequired();
        builder.Property(u => u.LastModifiedUtc).IsRequired();
        builder.Property(u => u.CreatedBy).HasMaxLength(100);
        builder.Property(u => u.LastModifiedBy).HasMaxLength(100);

        builder.Ignore(u => u.DomainEvents);
    }
}