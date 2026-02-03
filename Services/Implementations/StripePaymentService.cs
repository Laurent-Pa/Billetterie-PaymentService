using PaymentService.Models;
using PaymentService.Services.Interfaces;
using Stripe;

namespace PaymentService.Services.Implementations
{
    public class StripePaymentService : IPaymentService
    {
        private readonly string _apiKey;
        private readonly ILogger<StripePaymentService> _logger;

        public StripePaymentService(IConfiguration configuration, ILogger<StripePaymentService> logger)
        {
            _apiKey = configuration["Stripe:SecretKey"]
                ?? throw new InvalidOperationException("Stripe API key not configured");
            _logger = logger;

            StripeConfiguration.ApiKey = _apiKey;
        }

        public async Task<PaymentResult> ProcessPaymentAsync(
            decimal amount,
            string currency,
            string orderId,
            string? paymentMethodId = null,
            string? customerEmail = null,
            string? description = null)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(paymentMethodId))
                {
                    throw new InvalidOperationException("Stripe PaymentMethodId is required for payment confirmation");
                }

                PaymentIntentCreateOptions options = new()
                {
                    Amount = (long)(amount * 100), // Stripe utilise les centimes
                    Currency = currency.ToLower(),
                    Description = description ?? $"Order {orderId}",
                    PaymentMethod = paymentMethodId,
                    Confirm = true,
                    ReceiptEmail = customerEmail,
                    PaymentMethodTypes = new List<string> { "card" },
                    Metadata = new Dictionary<string, string>
                    {
                        { "order_id", orderId }
                    },
                    // Auto-confirmation pour simplifier (en prod confirmation côté client)
                    //Confirm = true,
                    //AutomaticPaymentMethods = new PaymentIntentAutomaticPaymentMethodsOptions
                    //{
                    //    Enabled = true,
                    //    AllowRedirects = "never"
                    //}
                };

                var service = new PaymentIntentService();
                var paymentIntent = await service.CreateAsync(options);

                _logger.LogInformation("Payment processed: {PaymentIntentId} for order {OrderId}",
                    paymentIntent.Id, orderId);

                return new PaymentResult
                {
                    IsSuccess = true,
                    PaymentId = paymentIntent.Id,
                    TransactionId = paymentIntent.Id,
                    Status = paymentIntent.Status,
                    Amount = amount,
                    Currency = currency,
                    ClientSecret = paymentIntent.ClientSecret,
                    ErrorMessage = paymentIntent.Status != "succeeded"
                        ? $"Payment status: {paymentIntent.Status}"
                        : null
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Stripe payment failed for order {OrderId}", orderId);

                return new PaymentResult
                {
                    IsSuccess = false,
                    PaymentId = string.Empty,
                    TransactionId = string.Empty,
                    Status = "Failed",
                    Amount = amount,
                    Currency = currency,
                    ErrorMessage = ex.Message
                };
            }
        }

        public async Task<PaymentResult> GetPaymentStatusAsync(string paymentId)
        {
            try
            {
                var service = new PaymentIntentService();
                var paymentIntent = await service.GetAsync(paymentId);

                return new PaymentResult
                {
                    IsSuccess = paymentIntent.Status == "succeeded",
                    PaymentId = paymentIntent.Id,
                    TransactionId = paymentIntent.Id,
                    Status = paymentIntent.Status,
                    Amount = paymentIntent.Amount / 100m, // Reconvertir en euros
                    Currency = paymentIntent.Currency.ToUpper(),
                    ErrorMessage = null
                };
            }
            catch (StripeException ex)
            {
                _logger.LogError(ex, "Failed to get payment status for {PaymentId}", paymentId);

                return new PaymentResult
                {
                    IsSuccess = false,
                    PaymentId = paymentId,
                    Status = "Error",
                    ErrorMessage = ex.Message
                };
            }
        }
    }
}