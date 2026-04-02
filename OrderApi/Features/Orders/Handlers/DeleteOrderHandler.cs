using MediatR;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Features.Orders.Commands;

namespace OrderApi.Features.Orders.Handlers;

public class DeleteOrderHandler : IRequestHandler<DeleteOrderCommand, bool>
{
    private readonly OrderDbContext _db;
    public DeleteOrderHandler(OrderDbContext db) { _db = db; }
    public async Task<bool> Handle(DeleteOrderCommand request, CancellationToken cancellationToken)
    {
        var order = await _db.Orders.FirstOrDefaultAsync(o => o.Id == request.Id, cancellationToken);
        if (order is null) return false;

        if (order.Status != SharedLibrary.Enums.OrderStatus.Pending && order.Status != SharedLibrary.Enums.OrderStatus.Cancelled)
            throw new InvalidOperationException("Only Pending or Cancelled orders can be deleted.");

        _db.Orders.Remove(order);
        await _db.SaveChangesAsync(cancellationToken);

        return true;
    }
}
