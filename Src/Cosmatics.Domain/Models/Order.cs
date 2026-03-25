using Cosmatics.Domain.Enums;
using Cosmatics.Domain.Models;
using System.ComponentModel.DataAnnotations.Schema;

namespace Cosmatics.Models;

public class Order : BaseEntity
{
    public int Id { get; set; }
    public int UserId { get; set; }
    public User? User { get; set; }
    public DateTime OrderDate { get; set; } = DateTime.UtcNow;
    [Column(TypeName = "decimal(18,2)")]
    public decimal TotalAmount { get; set; }
    public OrderStatus Status { get; set; } = OrderStatus.Pending;
    public List<OrderItem> OrderItems { get; set; } = new();

    public Payment? Payment { get; set; }
    public void MarkAsPaid()
    {
        if (Status == OrderStatus.Paid)
            return;

        Status = OrderStatus.Paid;
    }

    public void MarkAsFailed()
    {
        if (Status == OrderStatus.Paid)
            throw new Exception("Cannot fail a paid order");

        Status = OrderStatus.Failed;
    }
}
