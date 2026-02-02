namespace PaymentService.DTOs
{
    public class PaymentResponseDto
    {
        public string PaymentIntentId { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty; // "Succeeded", "Failed", "Pending"
        public decimal Amount { get; set; }
        public string Currency { get; set; } = string.Empty;
        public string OrderId { get; set; } = string.Empty;
        public DateTime ProcessedAt { get; set; }
        public string? ErrorMessage { get; set; }
        public string? TransactionId { get; set; } // ID Stripe ou autre provider
        public string? ClientSecret { get; set; } // Pour confirmation côté client
    }
}
