using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Features.Orders.Commands;

namespace OrderApi.Features.Orders.Handlers;

public class CreateOrderHandler : IRequestHandler<CreateOrderCommand, OrderApi.DTOs.OrderResponseDto>
{
    private readonly OrderDbContext _db;
    public CreateOrderHandler(OrderDbContext db) { _db = db; }
    public async Task<OrderApi.DTOs.OrderResponseDto> Handle(CreateOrderCommand request, CancellationToken cancellationToken)
    {
        var dto = request.Dto;
        var order = new OrderApi.Models.Order
        {
            CustomerName = dto.CustomerName,
            CustomerEmail = dto.CustomerEmail,
            Currency = dto.Currency,
            Notes = dto.Notes,
            Items = dto.Items.Select(i => new OrderApi.Models.OrderItem
            {
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice
            }).ToList()
        };

        order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

        await _db.Orders.AddAsync(order, cancellationToken);
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
