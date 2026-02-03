using PaymentService.Models;

namespace PaymentService.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(
            decimal amount,
            string currency,
            string orderId,
            string? paymentMethodId = null,
            string? customerEmail = null,
            string? description = null);
        Task<PaymentResult> GetPaymentStatusAsync(string paymentId);
    }
}
