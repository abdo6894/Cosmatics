using Cosmatics.Models;
using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;

namespace Cosmatics.Infrastructure.Persistense.Data.Configuration
{
    internal partial class PaymentConfiguration : IEntityTypeConfiguration<Payment>
    {
        public void Configure(EntityTypeBuilder<Payment> builder)
        {
           
                builder.Property(p => p.Status)
                .HasConversion<string>();

                builder
                    .HasIndex(p => p.ExternalPaymentId)
                    .IsUnique();

                builder
                .HasOne(p => p.Order)
                .WithOne(o => o.Payment)
                .HasForeignKey<Payment>(p => p.OrderId);

                        builder
                 .HasQueryFilter(p => !p.IsDeleted);

        }
    }
}
