using PaymentService.Models;

namespace PaymentService.Services.Interfaces
{
    public interface IPaymentService
    {
        Task<PaymentResult> ProcessPaymentAsync(decimal amount, string currency, string orderId);
        Task<PaymentResult> GetPaymentStatusAsync(string paymentId);
    }
}
