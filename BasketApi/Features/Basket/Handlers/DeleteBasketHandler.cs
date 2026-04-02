using BasketApi.Features.Basket.Commands;
using BasketApi.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BasketApi.Features.Basket.Handlers;

public class DeleteBasketHandler : IRequestHandler<DeleteBasketCommand, bool>
{
    private readonly BasketDbContext _db;
    public DeleteBasketHandler(BasketDbContext db) { _db = db; }
    public async Task<bool> Handle(DeleteBasketCommand request, CancellationToken cancellationToken)
    {
        var basket = await _db.ShoppingCarts.FirstOrDefaultAsync(c => c.CustomerId == request.CustomerId, cancellationToken);
        if (basket is null) return false;
        _db.ShoppingCarts.Remove(basket);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
