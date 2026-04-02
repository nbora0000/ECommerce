using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PaymentApi.Data;
using PaymentApi.Features.Payments.Queries;

namespace PaymentApi.Features.Payments.Handlers;

public class GetAllPaymentsHandler : IRequestHandler<GetAllPaymentsQuery, (IEnumerable<PaymentApi.DTOs.PaymentResponseDto>, int)>
{
    private readonly PaymentDbContext _db;
    public GetAllPaymentsHandler(PaymentDbContext db) { _db = db; }
    public async Task<(IEnumerable<PaymentApi.DTOs.PaymentResponseDto>, int)> Handle(GetAllPaymentsQuery request, CancellationToken cancellationToken)
    {
        var statusParam = new SqlParameter("@Status", (object?)request.Status?.ToString() ?? DBNull.Value);
        var pageParam = new SqlParameter("@Page", request.Page);
        var pageSizeParam = new SqlParameter("@PageSize", request.PageSize);

        var payments = await _db.Payments
            .FromSqlRaw("EXEC [dbo].[sp_GetAllPayments] @Status, @Page, @PageSize", statusParam, pageParam, pageSizeParam)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalCount = await GetPaymentCountAsync(request.Status, cancellationToken);
        return (payments.Select(MapToDto), totalCount);
    }

    private async Task<int> GetPaymentCountAsync(SharedLibrary.Enums.PaymentStatus? status, CancellationToken cancellationToken)
    {
        var param = new SqlParameter("@Status", (object?)status?.ToString() ?? DBNull.Value);
        using var command = _db.Database.GetDbConnection().CreateCommand();
        command.CommandText = "EXEC [dbo].[sp_GetPaymentCount] @Status";
        command.Parameters.Add(param);
        await _db.Database.OpenConnectionAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    private static PaymentApi.DTOs.PaymentResponseDto MapToDto(PaymentApi.Models.Payment p) => new()
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
