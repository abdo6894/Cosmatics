using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmatics.Application.DTOs.PaymentDto
{
    public class PaymentResult
    {
        public bool Success { get; set; }
        public string? CheckoutUrl { get; set; }
        public string? ExternalPaymentId { get; set; }
        public string? Error { get; set; }
    }

}

