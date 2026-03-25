using Cosmatics.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cosmatics.Infrastructure.Persistense.Data.Configuration
{
    internal partial class OrderConfiguration
    {
        public class ProductConfiguration : IEntityTypeConfiguration<Product>
        {
            public void Configure(EntityTypeBuilder<Product> builder)
            {
                // Decimal precision
                builder
                    .Property(p => p.Price)
                    .HasColumnType("decimal(18,2)");


            }
        }

    }
}
