using InventoryApi.Features.Inventory.Commands;
using InventoryApi.Data;
using InventoryApi.Models;
using MediatR;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace InventoryApi.Features.Inventory.Handlers;

public class ReserveInventoryHandler : IRequestHandler<ReserveInventoryCommand, bool>
{
    private readonly InventoryDbContext _db;
    private readonly ILogger<ReserveInventoryHandler> _logger;
    public ReserveInventoryHandler(InventoryDbContext db, ILogger<ReserveInventoryHandler> logger) { _db = db; _logger = logger; }

    public async Task<bool> Handle(ReserveInventoryCommand request, CancellationToken cancellationToken)
    {
        var item = await _db.Stock.FirstOrDefaultAsync(s => s.ProductId == request.ProductId, cancellationToken);
        if (item == null || item.QuantityAvailable < request.Quantity)
        {
            return false;
        }

        item.QuantityAvailable -= request.Quantity;
        await _db.SaveChangesAsync(cancellationToken);

        _logger.LogInformation("Reserved {Quantity} of product {ProductId} for order {OrderId}", request.Quantity, request.ProductId, request.OrderId);
        return true;
    }
}
