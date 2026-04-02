using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Features.Orders.Commands;

namespace OrderApi.Features.Orders.Handlers;

public class UpdateOrderHandler : IRequestHandler<UpdateOrderCommand, OrderApi.DTOs.OrderResponseDto?>
{
    private readonly OrderDbContext _db;
    public UpdateOrderHandler(OrderDbContext db) { _db = db; }
    public async Task<OrderApi.DTOs.OrderResponseDto?> Handle(UpdateOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.Include(o => o.Items).FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (order is null) return null;

        if (order.Status == SharedLibrary.Enums.OrderStatus.Delivered || order.Status == SharedLibrary.Enums.OrderStatus.Cancelled)
            throw new InvalidOperationException($"Cannot update a {order.Status} order.");

        var dto = request.Dto;
        if (dto.CustomerName is not null) order.CustomerName = dto.CustomerName;
        if (dto.Notes is not null) order.Notes = dto.Notes;
        if (dto.Status.HasValue) order.Status = dto.Status.Value;

        order.UpdatedAt = DateTime.UtcNow;
        _db.Orders.Update(order);
        await _db.SaveChangesAsync(cancellationToken);

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
