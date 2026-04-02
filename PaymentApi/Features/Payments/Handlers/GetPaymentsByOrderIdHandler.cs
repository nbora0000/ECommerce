using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PaymentApi.Data;
using PaymentApi.Features.Payments.Queries;

namespace PaymentApi.Features.Payments.Handlers;

public class GetPaymentsByOrderIdHandler : IRequestHandler<GetPaymentsByOrderIdQuery, IEnumerable<PaymentApi.DTOs.PaymentResponseDto>>
{
    private readonly PaymentDbContext _db;
    public GetPaymentsByOrderIdHandler(PaymentDbContext db) { _db = db; }
    public async Task<IEnumerable<PaymentApi.DTOs.PaymentResponseDto>> Handle(GetPaymentsByOrderIdQuery request, CancellationToken cancellationToken)
    {
        var param = new SqlParameter("@OrderId", request.OrderId);
        var payments = await _db.Payments
            .FromSqlRaw("EXEC [dbo].[sp_GetPaymentsByOrderId] @OrderId", param)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        return payments.Select(p => new PaymentApi.DTOs.PaymentResponseDto
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
        });
    }
}
