using PaymentApi.DTOs;
using PaymentApi.Models;
using PaymentApi.Repositories;
using SharedLibrary.Enums;

namespace PaymentApi.Services
{
    /// <summary>
    /// Payment service — business logic layer between controller and repository.
    /// </summary>
    public class PaymentService : IPaymentService
    {
        private readonly IPaymentRepository _repository;
        private readonly PaymentApi.Data.PaymentDbContext _unitOfWork;
        private readonly ILogger<PaymentService> _logger;

        public PaymentService(IPaymentRepository repository, PaymentApi.Data.PaymentDbContext unitOfWork, ILogger<PaymentService> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<(IEnumerable<PaymentResponseDto> Payments, int TotalCount)> GetAllPaymentsAsync(
            PaymentStatus? status, int page, int pageSize)
        {
            var payments = await _repository.GetAllPaymentsAsync(status, page, pageSize);
            var totalCount = await _repository.GetPaymentCountAsync(status);
            return (payments.Select(MapToDto), totalCount);
        }

        public async Task<PaymentResponseDto?> GetPaymentByIdAsync(Guid id)
        {
            var payment = await _repository.GetPaymentByIdAsync(id);
            return payment is null ? null : MapToDto(payment);
        }

        public async Task<IEnumerable<PaymentResponseDto>> GetPaymentsByOrderIdAsync(Guid orderId)
        {
            var payments = await _repository.GetPaymentsByOrderIdAsync(orderId);
            return payments.Select(MapToDto);
        }

        public async Task<(PaymentResponseDto Payment, bool IsSuccess)> ProcessPaymentAsync(ProcessPaymentDto dto)
        {
            // Check if a completed payment already exists
            var exists = await _repository.CompletedPaymentExistsAsync(dto.OrderId);
            if (exists)
                throw new InvalidOperationException("A completed payment already exists for this order.");

            // Simulate payment gateway processing
            var transactionId = $"TXN-{Guid.NewGuid():N}".ToUpper()[..20];
            var isSuccess = SimulateGateway(dto.Method, dto.PaymentToken);

            var payment = new Payment
            {
                OrderId = dto.OrderId,
                Amount = dto.Amount,
                Currency = dto.Currency,
                Method = dto.Method,
                Status = isSuccess ? PaymentStatus.Completed : PaymentStatus.Failed,
                TransactionId = isSuccess ? transactionId : null,
                FailureReason = isSuccess ? null : "Gateway declined the transaction."
            };

            await _repository.AddAsync(payment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Payment {PaymentId} for Order {OrderId}: {Status}",
                payment.Id, dto.OrderId, payment.Status);

            return (MapToDto(payment), isSuccess);
        }

        public async Task<PaymentResponseDto?> RefundPaymentAsync(Guid id, RefundPaymentDto dto)
        {
            var payment = await _repository.GetByIdAsync(id);
            if (payment is null) return null;

            if (payment.Status != PaymentStatus.Completed)
                throw new InvalidOperationException(
                    $"Only Completed payments can be refunded. Current status: {payment.Status}.");

            payment.Status = PaymentStatus.Refunded;
            payment.RefundTransactionId = $"REF-{Guid.NewGuid():N}".ToUpper()[..20];
            payment.RefundedAt = DateTime.UtcNow;
            payment.UpdatedAt = DateTime.UtcNow;
            payment.FailureReason = dto.Reason;

            _repository.Update(payment);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Payment {PaymentId} refunded. RefundTxn: {RefundTxn}",
                id, payment.RefundTransactionId);

            return MapToDto(payment);
        }

        // ── Private helpers ──────────────────────────────────────────────────

        private static bool SimulateGateway(PaymentMethod method, string? token)
        {
            if (method == PaymentMethod.CashOnDelivery) return true;
            return !string.IsNullOrWhiteSpace(token);
        }

        private static PaymentResponseDto MapToDto(Payment p) => new()
        {
            Id = p.Id,
            OrderId = p.OrderId,
            Amount = p.Amount,
            Currency = p.Currency,
            Status = p.Status,
            Method = p.Method.ToString(),
            TransactionId = p.TransactionId,
            FailureReason = p.FailureReason,
            RefundTransactionId = p.RefundTransactionId,
            RefundedAt = p.RefundedAt,
            CreatedAt = p.CreatedAt,
            UpdatedAt = p.UpdatedAt
        };
    }
}
