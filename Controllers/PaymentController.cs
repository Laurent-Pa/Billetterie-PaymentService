using Microsoft.AspNetCore.Mvc;
using PaymentService.DTOs;
using PaymentService.Services.Interfaces;

namespace PaymentService.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;
        private readonly ILogger<PaymentsController> _logger;

        public PaymentsController(IPaymentService paymentService, ILogger<PaymentsController> logger)
        {
            _paymentService = paymentService;
            _logger = logger;
        }

        /// <summary>
        /// Process a new payment
        /// </summary>
        [HttpPost]
        [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status400BadRequest)]
        [ProducesResponseType(StatusCodes.Status500InternalServerError)]
        public async Task<ActionResult<PaymentResponseDto>> ProcessPayment([FromBody] PaymentRequestDto request)
        {
            if (!ModelState.IsValid)
            {
                return BadRequest(ModelState);
            }

            _logger.LogInformation("Processing payment for order {OrderId}, amount {Amount} {Currency}",
                request.OrderId, request.Amount, request.Currency);

            try
            {
                var result = await _paymentService.ProcessPaymentAsync(
                    request.Amount,
                    request.Currency,
                    request.OrderId
                );

                var response = new PaymentResponseDto
                {
                    PaymentIntentId = result.PaymentId,
                    Status = result.Status,
                    Amount = result.Amount,
                    Currency = result.Currency,
                    OrderId = request.OrderId,
                    ProcessedAt = DateTime.UtcNow,
                    ErrorMessage = result.ErrorMessage,
                    TransactionId = result.TransactionId,
                    ClientSecret = result.ClientSecret
                };

                if (result.IsSuccess)
                {
                    _logger.LogInformation("Payment succeeded: {PaymentId}", result.PaymentId);
                    return Ok(response);
                }
                else
                {
                    _logger.LogWarning("Payment failed for order {OrderId}: {Error}",
                        request.OrderId, result.ErrorMessage);
                    return BadRequest(response);
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Unexpected error processing payment for order {OrderId}", request.OrderId);
                return StatusCode(500, new { error = "Internal server error processing payment" });
            }
        }

        /// <summary>
        /// Get payment status by ID
        /// </summary>
        [HttpGet("{paymentId}")]
        [ProducesResponseType(typeof(PaymentResponseDto), StatusCodes.Status200OK)]
        [ProducesResponseType(StatusCodes.Status404NotFound)]
        public async Task<ActionResult<PaymentResponseDto>> GetPaymentStatus(string paymentId)
        {
            _logger.LogInformation("Getting payment status for {PaymentId}", paymentId);

            var result = await _paymentService.GetPaymentStatusAsync(paymentId);

            if (result.Status == "NotFound")
            {
                return NotFound(new { error = $"Payment {paymentId} not found" });
            }

            var response = new PaymentResponseDto
            {
                PaymentIntentId = result.PaymentId,
                Status = result.Status,
                Amount = result.Amount,
                Currency = result.Currency,
                OrderId = string.Empty, // On ne l'a pas forcément en récupération
                ProcessedAt = DateTime.UtcNow,
                ErrorMessage = result.ErrorMessage,
                TransactionId = result.TransactionId
            };

            return Ok(response);
        }

        /// <summary>
        /// Health check endpoint
        /// </summary>
        [HttpGet("health")]
        public IActionResult HealthCheck()
        {
            return Ok(new { status = "healthy", service = "PaymentService", timestamp = DateTime.UtcNow });
        }
    }
}