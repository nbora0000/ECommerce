using BasketApi.Models;
using SharedLibrary.Interfaces;

namespace BasketApi.Repositories
{
    /// <summary>
    /// Basket-specific repository with stored-procedure-backed query methods.
    /// </summary>
    public interface IBasketRepository : IRepository<ShoppingCart>
    {
        Task<ShoppingCart?> GetBasketByCustomerIdAsync(string customerId);
        Task RemoveCartItemsAsync(IEnumerable<CartItem> items);
    }
}
