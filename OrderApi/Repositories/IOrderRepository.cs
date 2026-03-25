using OrderApi.Models;
using SharedLibrary.Enums;
using SharedLibrary.Interfaces;

namespace OrderApi.Repositories
{
    /// <summary>
    /// Order-specific repository — extends generic repository with stored-procedure-backed queries.
    /// </summary>
    public interface IOrderRepository : IRepository<Order>
    {
        Task<IEnumerable<Order>> GetAllOrdersAsync(OrderStatus? status, int page, int pageSize);
        Task<Order?> GetOrderWithItemsAsync(Guid orderId);
        Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status);
        Task<int> GetOrderCountAsync(OrderStatus? status);
    }
}
