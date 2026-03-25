using Cosmatics.Application.Common;
using Cosmatics.Application.DTOs.PaymentDto;
using Cosmatics.Domain.Enums;
using Cosmatics.Infrastructure.Persistense.Data;
using Cosmatics.Models;
using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;

namespace Cosmatics.Infrastructure.Services.Payment
{
    public class PaymobGateway : IPaymentGateway
    {
        private readonly IConfiguration _config;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly AppDbContext _context;
        private readonly ILogger<PaymobGateway> _logger;

        public string Name => "Paymob";

        public PaymobGateway(
            IConfiguration config,
            IHttpClientFactory httpClientFactory,
            AppDbContext context,
            ILogger<PaymobGateway> logger)
        {
            _config = config;
            _httpClientFactory = httpClientFactory;
            _context = context;
            _logger = logger;
        }

        public async Task<PaymentResult> CreateAsync(CreatePaymentRequest request)
        {
            
            var order = await _context.Orders
                .Include(o => o.User)
                .FirstOrDefaultAsync(o => o.Id == request.OrderId);

            if (order == null)
                return new PaymentResult { Success = false, Error = "Order not found" };

            if (order.Status == OrderStatus.Paid)
                return new PaymentResult { Success = false, Error = "Order already paid" };

            // 2. إنشاء سجل الدفع
            var payment = new Models.Payment
            {
                OrderId = order.Id,
                PaymentMethod = Name,
                Amount = order.TotalAmount,
                Currency = "EGP",
                Status = PaymentStatus.Pending
            };

            _context.Payments.Add(payment);
            await _context.SaveChangesAsync();

            var client = _httpClientFactory.CreateClient();
            var baseUrl = _config["PaymentsIntegration:Paymob:BaseUrl"];
            var apiKey = _config["PaymentsIntegration:Paymob:ApiKey"];
            var iframeId = _config["PaymentsIntegration:Paymob:IframeId"];
            var integrationId = int.Parse(_config["PaymentsIntegration:Paymob:IntegrationId"]!);

            try
            {
                _logger.LogInformation("Paymob: Authenticating for Order {OrderId}", order.Id);

                // 3. المصادقة
                var authResp = await client.PostAsJsonAsync($"{baseUrl}/auth/tokens",
                    new { api_key = apiKey });

                authResp.EnsureSuccessStatusCode();
                var authData = await authResp.Content.ReadFromJsonAsync<JsonElement>();
                var token = authData.GetProperty("token").GetString();

                
                _logger.LogInformation("Paymob: Creating order for Order {OrderId}", order.Id);

                var orderResp = await client.PostAsJsonAsync($"{baseUrl}/ecommerce/orders",
                    new
                    {
                        auth_token = token,
                        delivery_needed = false,
                        amount_cents = (long)(order.TotalAmount * 100),
                        currency = "EGP",
                        items = Array.Empty<object>()
                    });

                orderResp.EnsureSuccessStatusCode();
                var orderJson = await orderResp.Content.ReadFromJsonAsync<JsonElement>();
                var paymobOrderId = orderJson.GetProperty("id").GetInt32();

           
                _logger.LogInformation("Paymob: Creating payment key for Order {OrderId}", order.Id);

                var paymentResp = await client.PostAsJsonAsync($"{baseUrl}/acceptance/payment_keys",
                    new
                    {
                        auth_token = token,
                        amount_cents = (long)(order.TotalAmount * 100),
                        expiration = 3600,
                        order_id = paymobOrderId,
                        billing_data = new
                        {
                            email = order.User?.Email ?? "customer@example.com",
                            phone_number = order.User?.PhoneNumber ?? "01000000000",
                            city = "Cairo",
                            country = "EG",
                            state = "Cairo"
                        },
                        currency = "EGP",
                        integration_id = integrationId
                    });

                paymentResp.EnsureSuccessStatusCode();
                var paymentJson = await paymentResp.Content.ReadFromJsonAsync<JsonElement>();
                var paymentToken = paymentJson.GetProperty("token").GetString();


                payment.ExternalPaymentId = paymobOrderId.ToString();
                payment.RawResponse = paymentJson.ToString();
                await _context.SaveChangesAsync();

                var iframeUrl = $"{baseUrl}/acceptance/iframes/{iframeId}?payment_token={paymentToken}";

                return new PaymentResult
                {
                    Success = true,
                    CheckoutUrl = iframeUrl,
                    ExternalPaymentId = payment.ExternalPaymentId
                };
            }
            catch (HttpRequestException ex)
            {
                _logger.LogError(ex, "Paymob HTTP error for Order {OrderId}", order.Id);
                payment.MarkAsFailed();
                await _context.SaveChangesAsync();
                return new PaymentResult { Success = false, Error = "Payment gateway communication error" };
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Paymob JSON parsing error for Order {OrderId}", order.Id);
                payment.MarkAsFailed();
                await _context.SaveChangesAsync();
                return new PaymentResult { Success = false, Error = "Invalid response from payment gateway" };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Paymob unexpected error for Order {OrderId}", order.Id);
                payment.MarkAsFailed();
                await _context.SaveChangesAsync();
                return new PaymentResult { Success = false, Error = "An unexpected error occurred" };
            }
        }

        public async Task<bool> HandleWebhookAsync(string payload, IHeaderDictionary headers)
        {
            try
            {
                _logger.LogInformation("Paymob webhook received: {Payload}", payload);

                using var doc = JsonDocument.Parse(payload);
                var root = doc.RootElement;

             
                if (!ValidatePaymobSignature(payload, headers))
                {
                    _logger.LogWarning("Paymob webhook signature validation failed");
                    return false;
                }

              
                if (root.TryGetProperty("success", out var successElement) &&
                    successElement.GetBoolean() &&
                    root.TryGetProperty("order", out var orderElement) &&
                    orderElement.TryGetProperty("id", out var orderIdElement))
                {
                    var paymobOrderId = orderIdElement.ToString();

                    _logger.LogInformation("Paymob: Processing successful payment for Order ID {PaymobOrderId}", paymobOrderId);

                    var payment = await _context.Payments
                        .Include(p => p.Order)
                        .FirstOrDefaultAsync(p => p.ExternalPaymentId == paymobOrderId);

                    if (payment == null)
                    {
                        _logger.LogWarning("Paymob: Payment not found for Order ID {PaymobOrderId}", paymobOrderId);
                        return true; 
                    }

                    if (payment.Status == PaymentStatus.Paid)
                    {
                        _logger.LogInformation("Paymob: Payment {PaymentId} already paid", payment.Id);
                        return true;
                    }

                    payment.MarkAsPaid();
                    payment.RawResponse = payload;
                    payment.Order.MarkAsPaid();

                    await _context.SaveChangesAsync();

                    _logger.LogInformation("Paymob: Payment {PaymentId} marked as paid successfully", payment.Id);
                    return true;
                }

                _logger.LogInformation("Paymob webhook ignored: Not a successful transaction");
                return false;
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Paymob webhook: Invalid JSON payload");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Paymob webhook: Unexpected error");
                return false;
            }
        }

        private bool ValidatePaymobSignature(string payload, IHeaderDictionary headers)
        {

            var hmacSecret = _config["Paymob:HmacSecret"];
            if (string.IsNullOrEmpty(hmacSecret))
                return true;
  
            if (!headers.TryGetValue("hmac", out var signatureHeader))
                return false;

            var providedSignature = signatureHeader.ToString();

            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes(hmacSecret));
            var computedHash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
            var computedSignature = BitConverter.ToString(computedHash).Replace("-", "").ToLower();

            return computedSignature == providedSignature;
        }
    }
}