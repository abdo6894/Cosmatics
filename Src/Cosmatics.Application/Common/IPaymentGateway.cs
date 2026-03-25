
using Cosmatics.Application.DTOs.PaymentDto;
using Microsoft.AspNetCore.Http;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmatics.Application.Common
{
    public interface IPaymentGateway
    {
        string Name { get; }
        Task<PaymentResult> CreateAsync(CreatePaymentRequest request);
        Task<bool> HandleWebhookAsync(string payload, IHeaderDictionary headers);
    }


}
