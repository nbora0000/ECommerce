using PaymentApi.Models;
using SharedLibrary.Enums;
using SharedLibrary.Interfaces;

namespace PaymentApi.Repositories
{
    /// <summary>
    /// Payment-specific repository with stored-procedure-backed query methods.
    /// </summary>
    public interface IPaymentRepository : IRepository<Payment>
    {
        Task<IEnumerable<Payment>> GetAllPaymentsAsync(PaymentStatus? status, int page, int pageSize);
        Task<Payment?> GetPaymentByIdAsync(Guid id);
        Task<IEnumerable<Payment>> GetPaymentsByOrderIdAsync(Guid orderId);
        Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status);
        Task<int> GetPaymentCountAsync(PaymentStatus? status);
        Task<bool> CompletedPaymentExistsAsync(Guid orderId);
    }
}
