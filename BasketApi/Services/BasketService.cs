using BasketApi.Models;
using BasketApi.Repositories;

namespace BasketApi.Services
{
    /// <summary>
    /// Basket service — business logic layer between controller and repository.
    /// </summary>
    public class BasketService : IBasketService
    {
        private readonly IBasketRepository _repository;
        private readonly BasketApi.Data.BasketDbContext _unitOfWork;

        public BasketService(IBasketRepository repository, BasketApi.Data.BasketDbContext unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<ShoppingCart> GetBasketAsync(string customerId)
        {
            var basket = await _repository.GetBasketByCustomerIdAsync(customerId);
            return basket ?? new ShoppingCart { CustomerId = customerId };
        }

        public async Task<ShoppingCart> UpdateBasketAsync(ShoppingCart basket)
        {
            var existing = await _repository.GetBasketByCustomerIdAsync(basket.CustomerId);

            if (existing is null)
            {
                await _repository.AddAsync(basket);
            }
            else
            {
                await _repository.RemoveCartItemsAsync(existing.Items);
                existing.Items = basket.Items;
            }

            await _unitOfWork.SaveChangesAsync();
            return basket;
        }

        public async Task<bool> DeleteBasketAsync(string customerId)
        {
            var basket = await _repository.GetByIdAsync(customerId);
            if (basket is null) return false;

            _repository.Delete(basket);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
