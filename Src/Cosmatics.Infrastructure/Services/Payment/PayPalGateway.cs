using Cosmatics.Application.Common;
using Cosmatics.Application.DTOs.PaymentDto;
using Cosmatics.Domain.Enums;
using Cosmatics.Infrastructure.Persistense.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using System.Text;
using System.Text.Json;

namespace Cosmatics.Infrastructure.Services.Payment
{
    public class PayPalGateway : IPaymentGateway
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppDbContext _context;
        public string Name => "PayPal";

        public PayPalGateway(IConfiguration config, IHttpClientFactory httpClientFactory, AppDbContext context)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _context = context;
        }

        public async Task<PaymentResult> CreateAsync(CreatePaymentRequest request)
        {
            var order = await _context.Orders.FindAsync(request.OrderId);
            if (order == null)
                return new PaymentResult { Success = false, Error = "Order not found" };
            if (order.Status == OrderStatus.Paid)
                return new PaymentResult { Success = false, Error = "Order already paid" };

            
            var client = _httpClientFactory.CreateClient();
            var token = await GetAccessToken(client);

            client.DefaultRequestHeaders.Authorization = new AuthenticationHeaderValue("Bearer", token);

            try
            {
              
                var response = await client.PostAsJsonAsync(
                    $"{_config["PaymentsIntegration:PayPal:BaseUrl"]}/v2/checkout/orders",
                    new
                    {
                        intent = "CAPTURE",
                        purchase_units = new[]
                        {
                    new {
                        amount = new {
                            currency_code = "USD",
                            value = order.TotalAmount.ToString("F2")
                        }
                    }
                        }
                    });

                response.EnsureSuccessStatusCode();
                var json = await response.Content.ReadFromJsonAsync<JsonElement>();
                var paypalOrderId = json.GetProperty("id").GetString();

                var payment = new Models.Payment
                {
                    OrderId = order.Id,
                    PaymentMethod = Name,
                    Amount = order.TotalAmount,
                    Currency = "USD",
                    ExternalPaymentId = paypalOrderId!, 
                    Status = PaymentStatus.Pending
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();  

                var approvalUrl = json.GetProperty("links")
                    .EnumerateArray()
                    .First(x => x.GetProperty("rel").GetString() == "approve")
                    .GetProperty("href").GetString();

                return new PaymentResult
                {
                    Success = true,
                    CheckoutUrl = approvalUrl,
                    ExternalPaymentId = paypalOrderId
                };
            }
            catch (Exception ex)
            {
             
                var failedPayment = new Models.Payment
                {
                    OrderId = order.Id,
                    PaymentMethod = Name,
                    Amount = order.TotalAmount,
                    Currency = "USD",
                    Status = PaymentStatus.Failed,
                    RawResponse = ex.Message
                };
                _context.Payments.Add(failedPayment);
                await _context.SaveChangesAsync();

                return new PaymentResult { Success = false, Error = $"PayPal error: {ex.Message}" };
            }
        }

        public async Task<bool> HandleWebhookAsync(string payload, IHeaderDictionary headers)
        {
            try
            {
                using var doc = JsonDocument.Parse(payload);
                var root = doc.RootElement;
                var eventType = root.GetProperty("event_type").GetString();

              
                if (eventType == "CHECKOUT.ORDER.APPROVED" || eventType == "PAYMENT.CAPTURE.COMPLETED")
                {
                   
                    var resource = root.GetProperty("resource");
                    var paypalOrderId = resource.GetProperty("id").GetString();

                  
                    var payment = await _context.Payments
                        .Include(p => p.Order)
                        .FirstOrDefaultAsync(p => p.ExternalPaymentId == paypalOrderId);

                    if (payment == null || payment.Status == PaymentStatus.Paid)
                        return true;

                    payment.MarkAsPaid();
                    payment.Order.MarkAsPaid();
                    await _context.SaveChangesAsync();
                    return true;
                }

                return false;
            }
            catch (Exception)
            {
                return false;
            }
        }

        private async Task<string> GetAccessToken(HttpClient client)
        {
   
            var auth = Convert.ToBase64String(
                Encoding.UTF8.GetBytes($"{_config["PaymentsIntegration:PayPal:ClientId"]}:{_config["PaymentsIntegration:PayPal:ClientSecret"]}"));

            var request = new HttpRequestMessage(HttpMethod.Post, $"{_config["PaymentsIntegration:PayPal:BaseUrl"]}/v1/oauth2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", auth);
            request.Content = new StringContent("grant_type=client_credentials", Encoding.UTF8, "application/x-www-form-urlencoded");

            var response = await client.SendAsync(request);
            response.EnsureSuccessStatusCode();
            var json = await response.Content.ReadFromJsonAsync<JsonElement>();
            return json.GetProperty("access_token").GetString()!;
        }
    }
}