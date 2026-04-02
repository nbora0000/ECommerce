using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Features.Orders.Queries;

namespace OrderApi.Features.Orders.Handlers;

public class GetOrderByIdHandler : IRequestHandler<GetOrderByIdQuery, OrderApi.DTOs.OrderResponseDto?>
{
    private readonly OrderDbContext _db;
    public GetOrderByIdHandler(OrderDbContext db) { _db = db; }
    public async Task<OrderApi.DTOs.OrderResponseDto?> Handle(GetOrderByIdQuery request, CancellationToken cancellationToken)
    {
        var param = new SqlParameter("@OrderId", request.Id);
        var orders = await _db.Orders.FromSqlRaw("EXEC [dbo].[sp_GetOrderById] @OrderId", param).ToListAsync(cancellationToken);
        var order = orders.FirstOrDefault();
        if (order is null) return null;

        await _db.Entry(order).Collection(o => o.Items).LoadAsync(cancellationToken);

        return new OrderApi.DTOs.OrderResponseDto
        {
            Id = order.Id,
            CustomerName = order.CustomerName,
            CustomerEmail = order.CustomerEmail,
            TotalAmount = order.TotalAmount,
            Currency = order.Currency,
            Status = order.Status,
            Notes = order.Notes,
            CreatedAt = order.CreatedAt,
            UpdatedAt = order.UpdatedAt,
            Items = order.Items.Select(i => new OrderApi.DTOs.OrderItemResponseDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        };
    }
}
