using PaymentApi.DTOs;
using SharedLibrary.Enums;

namespace PaymentApi.Services
{
    /// <summary>
    /// Payment service interface — business operations for the payment domain.
    /// </summary>
    public interface IPaymentService
    {
        Task<(IEnumerable<PaymentResponseDto> Payments, int TotalCount)> GetAllPaymentsAsync(PaymentStatus? status, int page, int pageSize);
        Task<PaymentResponseDto?> GetPaymentByIdAsync(Guid id);
        Task<IEnumerable<PaymentResponseDto>> GetPaymentsByOrderIdAsync(Guid orderId);
        Task<(PaymentResponseDto Payment, bool IsSuccess)> ProcessPaymentAsync(ProcessPaymentDto dto);
        Task<PaymentResponseDto?> RefundPaymentAsync(Guid id, RefundPaymentDto dto);
    }
}
