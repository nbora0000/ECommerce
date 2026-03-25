using OrderApi.DTOs;
using SharedLibrary.Enums;

namespace OrderApi.Services
{
    /// <summary>
    /// Service interface that encapsulates order business logic.
    /// </summary>
    public interface IOrderService
    {
        Task<(IEnumerable<OrderResponseDto> Orders, int TotalCount)> GetAllOrdersAsync(OrderStatus? status, int page, int pageSize);
        Task<OrderResponseDto?> GetOrderByIdAsync(Guid id);
        Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto);
        Task<OrderResponseDto?> UpdateOrderAsync(Guid id, UpdateOrderDto dto);
        Task<bool> DeleteOrderAsync(Guid id);
    }
}
