using Cosmatics.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cosmatics.Infrastructure.Persistense.Data.Configuration
{
    internal partial class OrderConfiguration
    {
        public class OrderItemConfiguration : IEntityTypeConfiguration<OrderItem>
        {
            public void Configure(EntityTypeBuilder<OrderItem> builder)
            {
                // Order - OrderItem
                builder
                    .HasOne(oi => oi.Order)
                    .WithMany(o => o.OrderItems)
                    .HasForeignKey(oi => oi.OrderId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Product - OrderItem
                builder
                    .HasOne(oi => oi.Product)
                    .WithMany()
                    .HasForeignKey(oi => oi.ProductId)
                    .OnDelete(DeleteBehavior.Restrict);

                // Decimal precision
                builder
                    .Property(oi => oi.Price)
                    .HasColumnType("decimal(18,2)");
            }
        }
    }
}
