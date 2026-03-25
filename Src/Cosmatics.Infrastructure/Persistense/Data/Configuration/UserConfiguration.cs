using Cosmatics.Domain.Enums;
using Cosmatics.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Microsoft.EntityFrameworkCore.Storage.ValueConversion;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection.Emit;
using System.Text;
using System.Threading.Tasks;

namespace Cosmatics.Infrastructure.Persistense.Data.Configuration
{
    public class UserConfiguration : IEntityTypeConfiguration<User>
    {
        public void Configure(EntityTypeBuilder<User> builder)
        {
       
            builder
                .HasIndex(u => u.Username)
                .IsUnique();

            builder
                .HasIndex(u => u.Email)
                .IsUnique();

            builder
                .HasIndex(u => new { u.CountryCode, u.PhoneNumber })
                .IsUnique();

            builder
                .HasIndex(u => u.RefreshToken);

            builder
                .Property(u => u.Username)
                .HasMaxLength(50)
                .IsRequired();

            builder
                .Property(u => u.Email)
                .HasMaxLength(100);

            builder
                .Property(u => u.CountryCode)
                .HasMaxLength(5)
                .IsRequired();

            builder
                .Property(u => u.PhoneNumber)
                .HasMaxLength(15)
                .IsRequired();

            builder.Property(u => u.Role)
                .HasConversion(new EnumToStringConverter<UserRole>())
                .HasDefaultValue(UserRole.Customer)
                .HasColumnType("nvarchar(20)")
                .HasSentinel(UserRole.Customer); 



            builder
                .Property(u => u.IsVerified)
                .HasDefaultValue(false);
        }
    }
}
