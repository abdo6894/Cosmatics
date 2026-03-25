namespace Cosmatics.Application.DTOs.PaymentDto
{
    public class CreatePaymentRequest
    {
        public int OrderId { get; set; }

        public string PaymentMethod { get; set; } = null!;
   
        public decimal? Amount { get; set; }
        public string SuccessUrl { get; set; } = null!;
        public string CancelUrl { get; set; } = null!;
    }


}
