using Cosmatics.Application.Common;
using Cosmatics.Application.DTOs.PaymentDto;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Text.Json;

namespace Cosmatics.API.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentController : ControllerBase
    {
        private readonly IPaymentGatewayFactory _factory;
        private readonly ILogger<PaymentController> _logger;

        public PaymentController(
            IPaymentGatewayFactory factory,
            ILogger<PaymentController> logger)
        {
            _factory = factory;
            _logger = logger;
        }


        [HttpPost("checkout")]
        [ProducesResponseType(typeof(PaymentResult), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        public async Task<IActionResult> Checkout([FromBody] CreatePaymentRequest request)
        {
            if (!ModelState.IsValid)
                return BadRequest(ModelState);

            try
            {
                _logger.LogInformation("Starting checkout for Order {OrderId} with method {PaymentMethod}",
                    request.OrderId, request.PaymentMethod);

                var gateway = _factory.Get(request.PaymentMethod);
                var result = await gateway.CreateAsync(request);

                if (!result.Success)
                {
                    _logger.LogWarning("Checkout failed for Order {OrderId}: {Error}",
                        request.OrderId, result.Error);
                    return BadRequest(result);
                }

                _logger.LogInformation("Checkout successful for Order {OrderId}, PaymentId: {PaymentId}",
                    request.OrderId, result.ExternalPaymentId);

                return Ok(result);
            }
            catch (NotSupportedException ex)
            {
                _logger.LogError(ex, "Unsupported payment method: {PaymentMethod}", request.PaymentMethod);
                return BadRequest(new { error = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error during checkout for Order {OrderId}", request.OrderId);
                return StatusCode(500, new { error = "An unexpected error occurred" });
            }
        }


        [HttpPost("webhook/{method}")]
        public async Task<IActionResult> Webhook(string method)
        {
            try
            {
             
                using var reader = new StreamReader(Request.Body);
                var payload = await reader.ReadToEndAsync();

                _logger.LogInformation("Received webhook from {Method}", method);

                var gateway = _factory.Get(method);
                var handled = await gateway.HandleWebhookAsync(payload, Request.Headers);

                if (handled)
                {
                    _logger.LogInformation("Webhook from {Method} handled successfully", method);
                    return Ok(new { received = true, handled = true });
                }

                _logger.LogWarning("Webhook from {Method} was not handled", method);
                return Ok(new { received = true, handled = false });
            }
            catch (NotSupportedException ex)
            {
                _logger.LogError(ex, "Unsupported webhook method: {Method}", method);
                return BadRequest(new { error = ex.Message });
            }
            catch (JsonException ex)
            {
                _logger.LogError(ex, "Invalid JSON in webhook payload from {Method}", method);
                return BadRequest(new { error = "Invalid payload format" });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error processing webhook from {Method}", method);
                return StatusCode(500, new { error = "Webhook processing failed" });
            }
        }


        [HttpGet("webhook/test")]
        [AllowAnonymous]
        public IActionResult TestWebhook()
        {
            return Ok(new
            {
                message = "Webhook endpoint is working",
                endpoints = new[]
                {
                    "POST /api/payment/webhook/stripe",
                    "POST /api/payment/webhook/paypal",
                    "POST /api/payment/webhook/paymob"
                }
            });
        }
    }
}