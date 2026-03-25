using Cosmatics.Application.Common;
using Cosmatics.Application.DTOs.PaymentDto;
using Cosmatics.Domain.Enums;
using Cosmatics.Infrastructure.Persistense.Data;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using Microsoft.EntityFrameworkCore;
using Stripe;
using Stripe.Checkout;
using Microsoft.Extensions.Logging;

namespace Cosmatics.Infrastructure.Services.Payment
{
    public class StripeGateway : IPaymentGateway
    {
        private readonly AppDbContext _context;
        private readonly IConfiguration _config;
        private readonly ILogger<StripeGateway> _logger;

        public string Name => "Stripe";

        public StripeGateway(
            AppDbContext context,
            IConfiguration config,
            ILogger<StripeGateway> logger)
        {
            _context = context;
            _config = config;
            _logger = logger;
            StripeConfiguration.ApiKey = _config["PaymentsIntegration:Stripe:SecretKey"];
        }

        public async Task<PaymentResult> CreateAsync(CreatePaymentRequest request)
        {
            try
            {
                _logger.LogInformation("Starting Stripe checkout for Order {OrderId}", request.OrderId);

                // 1. التحقق من وجود الطلب
                var order = await _context.Orders
                    .FirstOrDefaultAsync(o => o.Id == request.OrderId);

                if (order == null)
                {
                    _logger.LogWarning("Order {OrderId} not found", request.OrderId);
                    return new PaymentResult { Success = false, Error = "Order not found" };
                }

                if (order.Status == OrderStatus.Paid)
                {
                    _logger.LogWarning("Order {OrderId} is already paid", request.OrderId);
                    return new PaymentResult { Success = false, Error = "Order already paid" };
                }

                // 2. إنشاء جلسة Stripe أولاً (بدون Metadata)
                var options = new SessionCreateOptions
                {
                    Mode = "payment",
                    SuccessUrl = request.SuccessUrl,
                    CancelUrl = request.CancelUrl,
                    LineItems = new List<SessionLineItemOptions>
                    {
                        new SessionLineItemOptions
                        {
                            Quantity = 1,
                            PriceData = new SessionLineItemPriceDataOptions
                            {
                                Currency = "usd",
                                UnitAmount = (long)(order.TotalAmount * 100),
                                ProductData = new SessionLineItemPriceDataProductDataOptions
                                {
                                    Name = $"Order #{order.Id}",
                                    Description = $"Payment for order #{order.Id}"
                                }
                            }
                        }
                    }
                };

                var service = new SessionService();
                var session = await service.CreateAsync(options);

                _logger.LogInformation("Stripe session created with ID: {SessionId}", session.Id);

                // 3. إنشاء سجل الدفع في قاعدة البيانات (مع ExternalPaymentId)
                var payment = new Models.Payment
                {
                    OrderId = order.Id,
                    PaymentMethod = Name,
                    Amount = order.TotalAmount,
                    Currency = "usd",
                    ExternalPaymentId = session.Id, // تم التعيين هنا
                    Status = PaymentStatus.Pending,
                    CreatedAt = DateTime.UtcNow
                };

                _context.Payments.Add(payment);
                await _context.SaveChangesAsync();

                _logger.LogInformation("Payment record created with ID: {PaymentId} for Order {OrderId}",
                    payment.Id, order.Id);

                // 4. تحديث الجلسة بإضافة Metadata (اختياري - للـ Webhook)
                try
                {
                    var updateOptions = new SessionUpdateOptions
                    {
                        Metadata = new Dictionary<string, string>
                        {
                            { "paymentId", payment.Id.ToString() },
                            { "orderId", order.Id.ToString() }
                        }
                    };
                    await service.UpdateAsync(session.Id, updateOptions);
                }
                catch (Exception ex)
                {
                    // Stripe لا يسمح بتحديث Metadata بعد الإنشاء في بعض الحالات
                    // لذلك نتجاهل الخطأ ونكمل
                    _logger.LogWarning(ex, "Could not update session metadata, but continuing");
                }

                return new PaymentResult
                {
                    Success = true,
                    CheckoutUrl = session.Url,
                    ExternalPaymentId = session.Id
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating Stripe checkout for Order {OrderId}", request.OrderId);
                return new PaymentResult
                {
                    Success = false,
                    Error = "An unexpected error occurred"
                };
            }
        }

        public async Task<bool> HandleWebhookAsync(string payload, IHeaderDictionary headers)
        {
            try
            {
                _logger.LogInformation("Received Stripe webhook");

                // التحقق من التوقيع
                var stripeEvent = EventUtility.ConstructEvent(
                    payload,
                    headers["Stripe-Signature"],
                    _config["PaymentsIntegration:Stripe:WebhookSecret"]
                );

                _logger.LogInformation("Stripe webhook event type: {EventType}", stripeEvent.Type);

                // نهتم فقط بأحداث إتمام الدفع
                if (stripeEvent.Type != "checkout.session.completed")
                {
                    _logger.LogInformation("Ignoring webhook event type: {EventType}", stripeEvent.Type);
                    return false;
                }

                // استخراج بيانات الجلسة
                var session = stripeEvent.Data.Object as Session;
                if (session == null)
                {
                    _logger.LogWarning("Session object is null in webhook");
                    return false;
                }

                _logger.LogInformation("Processing checkout.session.completed for Session: {SessionId}", session.Id);

                // البحث عن الدفع باستخدام ExternalPaymentId
                var payment = await _context.Payments
                    .Include(p => p.Order)
                    .FirstOrDefaultAsync(p => p.ExternalPaymentId == session.Id);

                if (payment == null)
                {
                    _logger.LogWarning("Payment not found for ExternalPaymentId: {ExternalPaymentId}", session.Id);

                    // محاولة البحث باستخدام Metadata (كخطة بديلة)
                    if (session.Metadata != null && session.Metadata.TryGetValue("orderId", out var orderId))
                    {
                        if (int.TryParse(orderId, out var orderIdInt))
                        {
                            payment = await _context.Payments
                                .Include(p => p.Order)
                                .FirstOrDefaultAsync(p => p.OrderId == orderIdInt);
                        }
                    }

                    if (payment == null)
                    {
                        _logger.LogWarning("Payment not found using alternative methods");
                        return true; // نرجع true عشان Stripe ميفضلش يعيد الإرسال
                    }
                }

                // التأكد من أن الدفع لم يتم بالفعل
                if (payment.Status == PaymentStatus.Paid)
                {
                    _logger.LogInformation("Payment {PaymentId} already paid", payment.Id);
                    return true;
                }

                // تحديث حالة الدفع والطلب
                payment.MarkAsPaid();
                payment.RawResponse = payload;

                if (payment.Order != null)
                {
                    payment.Order.MarkAsPaid();
                    _logger.LogInformation("Order {OrderId} marked as paid", payment.Order.Id);
                }

                await _context.SaveChangesAsync();

                _logger.LogInformation("Payment {PaymentId} processed successfully", payment.Id);
                return true;
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe webhook signature verification failed");
                return false;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing Stripe webhook");
                return false;
            }
        }
    }
}