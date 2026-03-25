using Cosmatics.Models;
using Microsoft.EntityFrameworkCore;
using System.Reflection.Emit;

namespace Cosmatics.Infrastructure.Persistense.Data.Configuration
{
    internal partial class OrderConfiguration : IEntityTypeConfiguration<Order>
    {
        public void Configure(Microsoft.EntityFrameworkCore.Metadata.Builders.EntityTypeBuilder<Order> builder)
        {
            builder.HasOne(o => o.User)
                  .WithMany()
                  .HasForeignKey(o => o.UserId)
                  .OnDelete(DeleteBehavior.Restrict);


            builder
          .HasMany(o => o.OrderItems)
          .WithOne(oi => oi.Order)
          .HasForeignKey(oi => oi.OrderId)
          .OnDelete(DeleteBehavior.Cascade);

            builder
         .HasQueryFilter(o => !o.IsDeleted);

            builder.Property(o => o.Status)
             .HasConversion<string>();

            builder
                .Property(o => o.TotalAmount)
                .HasColumnType("decimal(18,2)");
        }
    }
}
