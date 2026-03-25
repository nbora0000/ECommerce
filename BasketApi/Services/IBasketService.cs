using BasketApi.Models;

namespace BasketApi.Services
{
    /// <summary>
    /// Basket service interface — business operations for the shopping cart domain.
    /// </summary>
    public interface IBasketService
    {
        Task<ShoppingCart> GetBasketAsync(string customerId);
        Task<ShoppingCart> UpdateBasketAsync(ShoppingCart basket);
        Task<bool> DeleteBasketAsync(string customerId);
    }
}
