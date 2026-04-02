using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Features.Orders.Queries;

namespace OrderApi.Features.Orders.Handlers;

public class GetAllOrdersHandler : IRequestHandler<GetAllOrdersQuery, (IEnumerable<OrderApi.DTOs.OrderResponseDto>, int)>
{
    private readonly OrderDbContext _db;
    public GetAllOrdersHandler(OrderDbContext db) { _db = db; }

    public async Task<(IEnumerable<OrderApi.DTOs.OrderResponseDto>, int)> Handle(GetAllOrdersQuery request, CancellationToken cancellationToken)
    {
        var statusParam = new SqlParameter("@Status", (object?)request.Status?.ToString() ?? DBNull.Value);
        var pageParam = new SqlParameter("@Page", request.Page);
        var pageSizeParam = new SqlParameter("@PageSize", request.PageSize);

        var orders = await _db.Orders
            .FromSqlRaw("EXEC [dbo].[sp_GetAllOrders] @Status, @Page, @PageSize", statusParam, pageParam, pageSizeParam)
            .AsNoTracking()
            .ToListAsync(cancellationToken);

        var totalCount = await GetOrderCountAsync(request.Status, cancellationToken);

        var dtos = new List<OrderApi.DTOs.OrderResponseDto>();
        foreach (var order in orders)
        {
            if (order != null)
            {
                await _db.Entry(order).Collection(o => o.Items).LoadAsync(cancellationToken);
                dtos.Add(MapToDto(order));
            }
        }

        return (dtos, totalCount);
    }

    private async Task<int> GetOrderCountAsync(SharedLibrary.Enums.OrderStatus? status, CancellationToken cancellationToken)
    {
        var param = new SqlParameter("@Status", (object?)status?.ToString() ?? DBNull.Value);
        using var command = _db.Database.GetDbConnection().CreateCommand();
        command.CommandText = "EXEC [dbo].[sp_GetOrderCount] @Status";
        command.Parameters.Add(param);
        await _db.Database.OpenConnectionAsync(cancellationToken);
        var result = await command.ExecuteScalarAsync(cancellationToken);
        return Convert.ToInt32(result);
    }

    private static OrderApi.DTOs.OrderResponseDto MapToDto(OrderApi.Models.Order o) => new()
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
        Items = o.Items.Select(i => new OrderApi.DTOs.OrderItemResponseDto
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
