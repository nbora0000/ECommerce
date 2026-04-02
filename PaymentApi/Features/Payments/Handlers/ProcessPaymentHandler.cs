using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PaymentApi.Data;
using PaymentApi.Features.Payments.Commands;
using PaymentApi.Models;

namespace PaymentApi.Features.Payments.Handlers;

public class ProcessPaymentHandler : IRequestHandler<ProcessPaymentCommand, (PaymentApi.DTOs.PaymentResponseDto, bool)>
{
    private readonly PaymentDbContext _db;
    public ProcessPaymentHandler(PaymentDbContext db) { _db = db; }

    public async Task<(PaymentApi.DTOs.PaymentResponseDto, bool)> Handle(ProcessPaymentCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;

        // Check if completed payment exists via stored proc
        var exists = false;
        var param = new SqlParameter("@OrderId", dto.OrderId);
        using (var command = _db.Database.GetDbConnection().CreateCommand())
        {
            command.CommandText = "EXEC [dbo].[sp_CheckCompletedPaymentExists] @OrderId";
            command.Parameters.Add(param);
            await _db.Database.OpenConnectionAsync(cancellationToken);
            var result = await command.ExecuteScalarAsync(cancellationToken);
            exists = Convert.ToInt32(result) == 1;
        }

        if (exists) throw new InvalidOperationException("A completed payment already exists for this order.");

        var transactionId = $"TXN-{Guid.NewGuid():N}".ToUpper()[..20];
        var isSuccess = SimulateGateway(dto.Method, dto.PaymentToken);

        var payment = new Payment
        {
            OrderId = dto.OrderId,
            Amount = dto.Amount,
            Currency = dto.Currency,
            Method = dto.Method,
            Status = isSuccess ? SharedLibrary.Enums.PaymentStatus.Completed : SharedLibrary.Enums.PaymentStatus.Failed,
            TransactionId = isSuccess ? transactionId : null,
            FailureReason = isSuccess ? null : "Gateway declined the transaction."
        };

        await _db.Payments.AddAsync(payment, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);

        return (MapToDto(payment), isSuccess);
    }

    private static bool SimulateGateway(PaymentApi.Models.PaymentMethod method, string? token)
    {
        if (method == PaymentApi.Models.PaymentMethod.CashOnDelivery) return true;
        return !string.IsNullOrWhiteSpace(token);
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
