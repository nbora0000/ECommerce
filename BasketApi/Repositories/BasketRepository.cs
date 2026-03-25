using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using BasketApi.Data;
using BasketApi.Models;
using SharedLibrary.Interfaces;

namespace BasketApi.Repositories
{
    /// <summary>
    /// Basket repository — stored procedures for reads, EF Core for writes.
    /// </summary>
    public class BasketRepository : IBasketRepository
    {
        private readonly BasketDbContext _context;

        public BasketRepository(BasketDbContext context)
        {
            _context = context;
        }

        // ── Stored-procedure-backed reads ────────────────────────────────────

        public async Task<ShoppingCart?> GetBasketByCustomerIdAsync(string customerId)
        {
            // The SP returns two result sets; EF FromSqlRaw reads only the first.
            // Load cart via SP, then eagerly load items via navigation.
            var param = new SqlParameter("@CustomerId", customerId);

            var carts = await _context.ShoppingCarts
                .FromSqlRaw("EXEC [dbo].[sp_GetBasketByCustomerId] @CustomerId", param)
                .ToListAsync();

            var cart = carts.FirstOrDefault();
            if (cart is not null)
            {
                await _context.Entry(cart)
                    .Collection(c => c.Items)
                    .LoadAsync();
            }

            return cart;
        }

        public async Task RemoveCartItemsAsync(IEnumerable<CartItem> items)
        {
            _context.CartItems.RemoveRange(items);
            await Task.CompletedTask;
        }

        // ── Generic IRepository<ShoppingCart> implementation ─────────────────

        public async Task<IEnumerable<ShoppingCart>> GetAllAsync()
        {
            return await _context.ShoppingCarts.Include(c => c.Items).AsNoTracking().ToListAsync();
        }

        public async Task<ShoppingCart?> GetByIdAsync(object id)
        {
            return await _context.ShoppingCarts
                .Include(c => c.Items)
                .FirstOrDefaultAsync(c => c.CustomerId == (string)id);
        }

        public async Task<IEnumerable<ShoppingCart>> FindAsync(Expression<Func<ShoppingCart, bool>> predicate)
        {
            return await _context.ShoppingCarts.Where(predicate).AsNoTracking().ToListAsync();
        }

        public async Task AddAsync(ShoppingCart entity)
        {
            await _context.ShoppingCarts.AddAsync(entity);
        }

        public void Update(ShoppingCart entity)
        {
            _context.ShoppingCarts.Update(entity);
        }

        public void Delete(ShoppingCart entity)
        {
            _context.ShoppingCarts.Remove(entity);
        }
    }
}
