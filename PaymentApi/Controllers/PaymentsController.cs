using Microsoft.AspNetCore.Mvc;
using PaymentApi.DTOs;
using PaymentApi.Services;
using SharedLibrary.Enums;

namespace PaymentApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class PaymentsController : ControllerBase
    {
        private readonly IPaymentService _paymentService;

        public PaymentsController(IPaymentService paymentService)
        {
            _paymentService = paymentService;
        }

        /// <summary>Get all payments</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<PaymentResponseDto>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] PaymentStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var (payments, totalCount) = await _paymentService.GetAllPaymentsAsync(status, page, pageSize);
            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            return Ok(payments);
        }

        /// <summary>Get payment by ID</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(PaymentResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var payment = await _paymentService.GetPaymentByIdAsync(id);
            if (payment is null) return NotFound(new { message = $"Payment {id} not found." });
            return Ok(payment);
        }

        /// <summary>Get all payments for a specific order</summary>
        [HttpGet("order/{orderId:guid}")]
        [ProducesResponseType(typeof(IEnumerable<PaymentResponseDto>), 200)]
        public async Task<IActionResult> GetByOrder(Guid orderId)
        {
            var payments = await _paymentService.GetPaymentsByOrderIdAsync(orderId);
            return Ok(payments);
        }

        /// <summary>Process a payment for an order</summary>
        [HttpPost("process")]
        [ProducesResponseType(typeof(PaymentResponseDto), 201)]
        [ProducesResponseType(400)]
        [ProducesResponseType(409)]
        public async Task<IActionResult> ProcessPayment([FromBody] ProcessPaymentDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (dto.Amount <= 0) return BadRequest(new { message = "Amount must be greater than zero." });

            try
            {
                var (payment, isSuccess) = await _paymentService.ProcessPaymentAsync(dto);

                if (!isSuccess)
                    return UnprocessableEntity(payment); // 422 — payment created but failed

                return CreatedAtAction(nameof(GetById), new { id = payment.Id }, payment);
            }
            catch (InvalidOperationException ex)
            {
                return Conflict(new { message = ex.Message });
            }
        }

        /// <summary>Refund a completed payment</summary>
        [HttpPut("{id:guid}/refund")]
        [ProducesResponseType(typeof(PaymentResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Refund(Guid id, [FromBody] RefundPaymentDto dto)
        {
            try
            {
                var payment = await _paymentService.RefundPaymentAsync(id, dto);
                if (payment is null) return NotFound(new { message = $"Payment {id} not found." });
                return Ok(payment);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
