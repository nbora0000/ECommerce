using BasketApi.Features.Basket.Queries;
using BasketApi.Data;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace BasketApi.Features.Basket.Handlers;

public class GetBasketHandler : IRequestHandler<GetBasketQuery, BasketApi.Models.ShoppingCart?>
{
    private readonly BasketDbContext _db;
    public GetBasketHandler(BasketDbContext db) { _db = db; }
    public async Task<BasketApi.Models.ShoppingCart?> Handle(GetBasketQuery request, CancellationToken cancellationToken)
    {
        var param = new SqlParameter("@CustomerId", request.CustomerId);
        var carts = await _db.ShoppingCarts.FromSqlRaw("EXEC [dbo].[sp_GetBasketByCustomerId] @CustomerId", param).ToListAsync(cancellationToken);
        var cart = carts.FirstOrDefault();
        if (cart is not null) await _db.Entry(cart).Collection(c => c.Items).LoadAsync(cancellationToken);
        return cart ?? new BasketApi.Models.ShoppingCart { CustomerId = request.CustomerId };
    }
}
