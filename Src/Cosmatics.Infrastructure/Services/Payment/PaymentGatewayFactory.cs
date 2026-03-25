using Cosmatics.Application.Common;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Cosmatics.Infrastructure.Services.Payment
{
    public class PaymentGatewayFactory : IPaymentGatewayFactory
    {
        private readonly IServiceProvider _serviceProvider;
        public PaymentGatewayFactory(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public IPaymentGateway Get(string method)
        {
            return method?.ToLower() switch
            {
                "paymob" => _serviceProvider.GetRequiredService<PaymobGateway>(),
                "paypal" => _serviceProvider.GetRequiredService<PayPalGateway>(),
                "stripe" => _serviceProvider.GetRequiredService<StripeGateway>(),
                _ => throw new NotSupportedException($"Payment method '{method}' is not supported.")
            };
        }

    }

}
