using MediatR;
using Microsoft.EntityFrameworkCore;
using PaymentApi.Data;
using PaymentApi.Models;
using PaymentApi.Features.Payments.Commands;

namespace PaymentApi.Features.Payments.Handlers;

public class RefundPaymentHandler : IRequestHandler<RefundPaymentCommand, PaymentApi.DTOs.PaymentResponseDto?>
{
    private readonly PaymentDbContext _db;
    public RefundPaymentHandler(PaymentDbContext db) { _db = db; }
    public async Task<PaymentApi.DTOs.PaymentResponseDto?> Handle(RefundPaymentCommand request, CancellationToken cancellationToken)
    {
        var payment = await _db.Payments.FindAsync(new object[] { request.Id }, cancellationToken);
        if (payment is null) return null;

        if (payment.Status != SharedLibrary.Enums.PaymentStatus.Completed)
            throw new InvalidOperationException($"Only Completed payments can be refunded. Current status: {payment.Status}.");

        payment.Status = SharedLibrary.Enums.PaymentStatus.Refunded;
        payment.RefundTransactionId = $"REF-{Guid.NewGuid():N}".ToUpper()[..20];
        payment.RefundedAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        payment.FailureReason = request.Dto.Reason;

        _db.Payments.Update(payment);
        await _db.SaveChangesAsync(cancellationToken);

        return MapToDto(payment);
    }

    private static PaymentApi.DTOs.PaymentResponseDto MapToDto(Payment p) => new()
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
