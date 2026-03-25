using Cosmatics.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cosmatics.Infrastructure.Persistense.Data.Configuration
{
    internal partial class OrderConfiguration
    {
        public class CartItemConfiguration : IEntityTypeConfiguration<CartItem>
        {
            public void Configure(EntityTypeBuilder<CartItem> builder)
            {
                // User - CartItem
                builder
                    .HasOne(ci => ci.User)
                    .WithMany()
                    .HasForeignKey(ci => ci.UserId)
                    .OnDelete(DeleteBehavior.Cascade);

                // Product - CartItem
                builder
                    .HasOne(ci => ci.Product)
                    .WithMany()
                    .HasForeignKey(ci => ci.ProductId)
                    .OnDelete(DeleteBehavior.Cascade);
            }
        }
    }
}
