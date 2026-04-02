using BasketApi.Features.Basket.Commands;
using BasketApi.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace BasketApi.Features.Basket.Handlers;

public class UpdateBasketHandler : IRequestHandler<UpdateBasketCommand, BasketApi.Models.ShoppingCart>
{
    private readonly BasketDbContext _db;
    public UpdateBasketHandler(BasketDbContext db) { _db = db; }
    public async Task<BasketApi.Models.ShoppingCart> Handle(UpdateBasketCommand request, CancellationToken cancellationToken)
    {
        var basket = request.Basket;
        var existing = await _db.ShoppingCarts.Include(c => c.Items).FirstOrDefaultAsync(c => c.CustomerId == basket.CustomerId, cancellationToken);
        if (existing is null)
        {
            await _db.ShoppingCarts.AddAsync(basket, cancellationToken);
        }
        else
        {
            _db.CartItems.RemoveRange(existing.Items);
            existing.Items = basket.Items;
            _db.ShoppingCarts.Update(existing);
        }

        await _db.SaveChangesAsync(cancellationToken);
        return basket;
    }
}
