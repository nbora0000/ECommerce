using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using PaymentApi.Data;
using PaymentApi.Models;
using SharedLibrary.Enums;
using SharedLibrary.Interfaces;

namespace PaymentApi.Repositories
{
    /// <summary>
    /// Payment repository — stored procedures for reads, EF Core for writes.
    /// </summary>
    public class PaymentRepository : IPaymentRepository
    {
        private readonly PaymentDbContext _context;

        public PaymentRepository(PaymentDbContext context)
        {
            _context = context;
        }

        // ── Stored-procedure-backed reads ────────────────────────────────────

        public async Task<IEnumerable<Payment>> GetAllPaymentsAsync(PaymentStatus? status, int page, int pageSize)
        {
            var statusParam = new SqlParameter("@Status", (object?)status?.ToString() ?? DBNull.Value);
            var pageParam = new SqlParameter("@Page", page);
            var pageSizeParam = new SqlParameter("@PageSize", pageSize);

            return await _context.Payments
                .FromSqlRaw("EXEC [dbo].[sp_GetAllPayments] @Status, @Page, @PageSize",
                    statusParam, pageParam, pageSizeParam)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Payment?> GetPaymentByIdAsync(Guid id)
        {
            var param = new SqlParameter("@PaymentId", id);

            var results = await _context.Payments
                .FromSqlRaw("EXEC [dbo].[sp_GetPaymentById] @PaymentId", param)
                .ToListAsync();

            return results.FirstOrDefault();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByOrderIdAsync(Guid orderId)
        {
            var param = new SqlParameter("@OrderId", orderId);

            return await _context.Payments
                .FromSqlRaw("EXEC [dbo].[sp_GetPaymentsByOrderId] @OrderId", param)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Payment>> GetPaymentsByStatusAsync(PaymentStatus status)
        {
            var param = new SqlParameter("@Status", status.ToString());

            return await _context.Payments
                .FromSqlRaw("EXEC [dbo].[sp_GetPaymentsByStatus] @Status", param)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<int> GetPaymentCountAsync(PaymentStatus? status)
        {
            var param = new SqlParameter("@Status", (object?)status?.ToString() ?? DBNull.Value);

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "EXEC [dbo].[sp_GetPaymentCount] @Status";
            command.Parameters.Add(param);

            await _context.Database.OpenConnectionAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result);
        }

        public async Task<bool> CompletedPaymentExistsAsync(Guid orderId)
        {
            var param = new SqlParameter("@OrderId", orderId);

            using var command = _context.Database.GetDbConnection().CreateCommand();
            command.CommandText = "EXEC [dbo].[sp_CheckCompletedPaymentExists] @OrderId";
            command.Parameters.Add(param);

            await _context.Database.OpenConnectionAsync();
            var result = await command.ExecuteScalarAsync();
            return Convert.ToInt32(result) == 1;
        }

        // ── Generic IRepository<Payment> implementation ──────────────────────

        public async Task<IEnumerable<Payment>> GetAllAsync()
        {
            return await _context.Payments.AsNoTracking().ToListAsync();
        }

        public async Task<Payment?> GetByIdAsync(object id)
        {
            return await _context.Payments.FindAsync((Guid)id);
        }

        public async Task<IEnumerable<Payment>> FindAsync(Expression<Func<Payment, bool>> predicate)
        {
            return await _context.Payments.Where(predicate).AsNoTracking().ToListAsync();
        }

        public async Task AddAsync(Payment entity)
        {
            await _context.Payments.AddAsync(entity);
        }

        public void Update(Payment entity)
        {
            _context.Payments.Update(entity);
        }

        public void Delete(Payment entity)
        {
            _context.Payments.Remove(entity);
        }
    }
}
