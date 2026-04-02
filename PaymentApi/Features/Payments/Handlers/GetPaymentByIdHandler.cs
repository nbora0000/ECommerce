using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PaymentApi.Data;
using PaymentApi.Features.Payments.Queries;

namespace PaymentApi.Features.Payments.Handlers;

public class GetPaymentByIdHandler : IRequestHandler<GetPaymentByIdQuery, PaymentApi.DTOs.PaymentResponseDto?>
{
    private readonly PaymentDbContext _db;
    public GetPaymentByIdHandler(PaymentDbContext db) { _db = db; }
    public async Task<PaymentApi.DTOs.PaymentResponseDto?> Handle(GetPaymentByIdQuery request, CancellationToken cancellationToken)
    {
        var param = new SqlParameter("@PaymentId", request.Id);
        var payments = await _db.Payments
            .FromSqlRaw("EXEC [dbo].[sp_GetPaymentById] @PaymentId", param)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var payment = payments.FirstOrDefault();
        if (payment is null) return null;

        return new PaymentApi.DTOs.PaymentResponseDto
        {
            Id = payment.Id,
            OrderId = payment.OrderId,
            Amount = payment.Amount,
            Currency = payment.Currency,
            Status = payment.Status,
            Method = payment.Method.ToString(),
            TransactionId = payment.TransactionId,
            FailureReason = payment.FailureReason,
            RefundTransactionId = payment.RefundTransactionId,
            RefundedAt = payment.RefundedAt,
            CreatedAt = payment.CreatedAt,
            UpdatedAt = payment.UpdatedAt
        };
    }
}
