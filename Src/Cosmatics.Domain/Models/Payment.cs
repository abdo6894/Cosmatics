using Cosmatics.Domain.Enums;
using Cosmatics.Domain.Models;

namespace Cosmatics.Models
{
    public class Payment : BaseEntity
    {
        public int Id { get; set; }

        public int OrderId { get; set; }
        public Order Order { get; set; }

        public string PaymentMethod { get; set; } = null!;

        public string ExternalPaymentId { get; set; } = null!;

        public decimal Amount { get; set; }

        public string Currency { get; set; } = "USD";

        public PaymentStatus Status { get; set; } = PaymentStatus.Pending;

        public DateTime? PaidAt { get; set; }

        public string? RawResponse { get; set; }
        public void MarkAsPaid()
        {
            if (Status == PaymentStatus.Paid)
                return;

            Status = PaymentStatus.Paid;
            PaidAt = DateTime.UtcNow;
        }

        public void MarkAsFailed()
        {
            if (Status == PaymentStatus.Paid)
                throw new Exception("Cannot fail a paid payment");

            Status = PaymentStatus.Failed;
        }
    }

}