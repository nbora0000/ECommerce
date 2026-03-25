using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Models;
using SharedLibrary.Enums;
using SharedLibrary.Interfaces;

namespace OrderApi.Repositories
{
    /// <summary>
    /// Order repository — uses stored procedures for reads and EF Core for writes.
    /// </summary>
    public class OrderRepository : IOrderRepository
    {
        private readonly OrderDbContext _context;

        public OrderRepository(OrderDbContext context)
        {
            _context = context;
        }

        // ── Stored-procedure-backed reads ────────────────────────────────────

        public async Task<IEnumerable<Order>> GetAllOrdersAsync(OrderStatus? status, int page, int pageSize)
        {
            var statusParam = new SqlParameter("@Status", (object?)status?.ToString() ?? DBNull.Value);
            var pageParam = new SqlParameter("@Page", page);
            var pageSizeParam = new SqlParameter("@PageSize", pageSize);

            return await _context.Orders
                .FromSqlRaw("EXEC [dbo].[sp_GetAllOrders] @Status, @Page, @PageSize",
                    statusParam, pageParam, pageSizeParam)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Order?> GetOrderWithItemsAsync(Guid orderId)
        {
            // Stored procedure returns two result sets; EF Core FromSqlRaw only reads the first.
            // We fetch the order via SP and eagerly load items via EF navigation.
            var param = new SqlParameter("@OrderId", orderId);

            var orders = await _context.Orders
                .FromSqlRaw("EXEC [dbo].[sp_GetOrderById] @OrderId", param)
                .ToListAsync();

            var order = orders.FirstOrDefault();
            if (order is not null)
            {
                await _context.Entry(order)
                    .Collection(o => o.Items)
                    .LoadAsync();
            }

            return order;
        }

        public async Task<IEnumerable<Order>> GetOrdersByStatusAsync(OrderStatus status)
        {
            var param = new SqlParameter("@Status", status.ToString());

            return await _context.Orders
                .FromSqlRaw("EXEC [dbo].[sp_GetOrdersByStatus] @Status", param)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetOrderCountAsync(OrderStatus? status)
        {
            var param = new SqlParameter("@Status", (object?)status?.ToString() ?? DBNull.Value);

            // sp_GetOrderCount returns a single scalar row
            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "EXEC [dbo].[sp_GetOrderCount] @Status";
            command.Parameters.Add(param);

            await _context.Database.OpenConnectionAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        // ── Generic IRepository<Order> implementation ────────────────────────

        public async Task<IEnumerable<Order>> GetAllAsync()
        {
            return await _context.Orders.Include(o => o.Items).AsNoTracking().ToListAsync();
        }

        public async Task<Order?> GetByIdAsync(object id)
        {
            return await _context.Orders.Include(o => o.Items)
                .FirstOrDefaultAsync(o => o.Id == (Guid)id);
        }

        public async Task<IEnumerable<Order>> FindAsync(Expression<Func<Order, bool>> predicate)
        {
            return await _context.Orders.Where(predicate).AsNoTracking().ToListAsync();
        }

        public async Task AddAsync(Order entity)
        {
            await _context.Orders.AddAsync(entity);
        }

        public void Update(Order entity)
        {
            _context.Orders.Update(entity);
        }

        public void Delete(Order entity)
        {
            _context.Orders.Remove(entity);
        }
    }
}
