namespace PaymentService.Models
{
    public class PaymentResult
    {
        public bool IsSuccess { get; set; }
        public string PaymentId { get; set; } = string.Empty;
        public string TransactionId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string? ErrorMessage { get; set; }
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string? ClientSecret { get; set; } // Pour confirmation côté client
    }
}
