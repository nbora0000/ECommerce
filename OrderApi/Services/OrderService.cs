using OrderApi.DTOs;
using OrderApi.Models;
using OrderApi.Repositories;
using SharedLibrary.Enums;

namespace OrderApi.Services
{
    /// <summary>
    /// Order service — business logic layer between controller and repository.
    /// </summary>
    public class OrderService : IOrderService
    {
        private readonly IOrderRepository _repository;
        private readonly OrderApi.Data.OrderDbContext _unitOfWork;
        private readonly ILogger<OrderService> _logger;

        public OrderService(IOrderRepository repository, OrderApi.Data.OrderDbContext unitOfWork, ILogger<OrderService> logger)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
            _logger = logger;
        }

        public async Task<(IEnumerable<OrderResponseDto> Orders, int TotalCount)> GetAllOrdersAsync(
            OrderStatus? status, int page, int pageSize)
        {
            var orders = await _repository.GetAllOrdersAsync(status, page, pageSize);
            var totalCount = await _repository.GetOrderCountAsync(status);

            // Items aren't loaded by the paginated SP; load them individually
            var dtos = new List<OrderResponseDto>();
            foreach (var order in orders)
            {
                var full = await _repository.GetOrderWithItemsAsync(order.Id);
                dtos.Add(MapToDto(full ?? order));
            }

            return (dtos, totalCount);
        }

        public async Task<OrderResponseDto?> GetOrderByIdAsync(Guid id)
        {
            var order = await _repository.GetOrderWithItemsAsync(id);
            return order is null ? null : MapToDto(order);
        }

        public async Task<OrderResponseDto> CreateOrderAsync(CreateOrderDto dto)
        {
            var order = new Order
            {
                CustomerName = dto.CustomerName,
                CustomerEmail = dto.CustomerEmail,
                Currency = dto.Currency,
                Notes = dto.Notes,
                Items = dto.Items.Select(i => new OrderItem
                {
                    ProductId = i.ProductId,
                    ProductName = i.ProductName,
                    Quantity = i.Quantity,
                    UnitPrice = i.UnitPrice
                }).ToList()
            };

            order.TotalAmount = order.Items.Sum(i => i.Quantity * i.UnitPrice);

            await _repository.AddAsync(order);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Order created: {OrderId} for {CustomerEmail}", order.Id, order.CustomerEmail);
            return MapToDto(order);
        }

        public async Task<OrderResponseDto?> UpdateOrderAsync(Guid id, UpdateOrderDto dto)
        {
            var order = await _repository.GetOrderWithItemsAsync(id);
            if (order is null) return null;

            if (order.Status is OrderStatus.Delivered or OrderStatus.Cancelled)
                throw new InvalidOperationException($"Cannot update a {order.Status} order.");

            if (dto.CustomerName is not null) order.CustomerName = dto.CustomerName;
            if (dto.Notes is not null) order.Notes = dto.Notes;
            if (dto.Status.HasValue) order.Status = dto.Status.Value;

            order.UpdatedAt = DateTime.UtcNow;
            _repository.Update(order);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Order updated: {OrderId}", order.Id);
            return MapToDto(order);
        }

        public async Task<bool> DeleteOrderAsync(Guid id)
        {
            var order = await _repository.GetByIdAsync(id);
            if (order is null) return false;

            if (order.Status != OrderStatus.Pending && order.Status != OrderStatus.Cancelled)
                throw new InvalidOperationException("Only Pending or Cancelled orders can be deleted.");

            _repository.Delete(order);
            await _unitOfWork.SaveChangesAsync();

            _logger.LogInformation("Order deleted: {OrderId}", id);
            return true;
        }

        // ── Mapping ──────────────────────────────────────────────────────────

        private static OrderResponseDto MapToDto(Order o) => new()
        {
            Id = o.Id,
            CustomerName = o.CustomerName,
            CustomerEmail = o.CustomerEmail,
            TotalAmount = o.TotalAmount,
            Currency = o.Currency,
            Status = o.Status,
            Notes = o.Notes,
            CreatedAt = o.CreatedAt,
            UpdatedAt = o.UpdatedAt,
            Items = o.Items.Select(i => new OrderItemResponseDto
            {
                Id = i.Id,
                ProductId = i.ProductId,
                ProductName = i.ProductName,
                Quantity = i.Quantity,
                UnitPrice = i.UnitPrice,
                TotalPrice = i.TotalPrice
            }).ToList()
        };
    }
}
