using PaymentService.Models;
using PaymentService.Services.Interfaces;

namespace PaymentService.Services.Implementations
{
    public class MockPaymentService : IPaymentService
    {
        private readonly Dictionary<string, PaymentResult> _payments = [];

        public async Task<PaymentResult> ProcessPaymentAsync(decimal amount, string currency, string orderId)
        {
            // Simulation d'un délai réseau
            await Task.Delay(500);

            string PaymentIntentId = Guid.NewGuid().ToString();
            string transactionId = $"mock_{DateTime.UtcNow.Ticks}";

            PaymentResult result = new()
            {
                IsSuccess = true,
                PaymentId = PaymentIntentId,
                TransactionId = transactionId,
                Status = "Succeeded",
                Amount = amount,
                Currency = currency,
                ErrorMessage = null
            };

            // Stocker pour GetPaymentStatusAsync
            _payments[PaymentIntentId] = result;

            return result;
        }

        public async Task<PaymentResult> GetPaymentStatusAsync(string paymentId)
        {
            await Task.Delay(100);

            if (_payments.TryGetValue(paymentId, out var payment))
            {
                return payment;
            }

            return new PaymentResult
            {
                IsSuccess = false,
                PaymentId = paymentId,
                Status = "NotFound",
                ErrorMessage = "Payment not found"
            };
        }
    }
}