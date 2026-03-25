using System.Linq.Expressions;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using CatalogApi.Data;
using CatalogApi.Models;
using SharedLibrary.Interfaces;

namespace CatalogApi.Repositories
{
    /// <summary>
    /// Product repository — stored procedures for reads, EF Core for writes.
    /// </summary>
    public class ProductRepository : IProductRepository
    {
        private readonly CatalogDbContext _context;

        public ProductRepository(CatalogDbContext context)
        {
            _context = context;
        }

        // ── Stored-procedure-backed reads ────────────────────────────────────

        public async Task<IEnumerable<Product>> GetAllProductsAsync(string? category)
        {
            var param = new SqlParameter("@Category", (object?)category ?? DBNull.Value);

            return await _context.Products
                .FromSqlRaw("EXEC [dbo].[sp_GetAllProducts] @Category", param)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            var param = new SqlParameter("@ProductId", id);

            var results = await _context.Products
                .FromSqlRaw("EXEC [dbo].[sp_GetProductById] @ProductId", param)
                .ToListAsync();

            return results.FirstOrDefault();
        }

        public async Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category)
        {
            var param = new SqlParameter("@Category", category);

            return await _context.Products
                .FromSqlRaw("EXEC [dbo].[sp_GetProductsByCategory] @Category", param)
                .AsNoTracking()
                .ToListAsync();
        }

        public async Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm)
        {
            var param = new SqlParameter("@SearchTerm", searchTerm);

            return await _context.Products
                .FromSqlRaw("EXEC [dbo].[sp_SearchProducts] @SearchTerm", param)
                .AsNoTracking()
                .ToListAsync();
        }

        public bool ProductExists(int id)
        {
            return _context.Products.Any(e => e.Id == id);
        }

        // ── Generic IRepository<Product> implementation ──────────────────────

        public async Task<IEnumerable<Product>> GetAllAsync()
        {
            return await _context.Products.AsNoTracking().ToListAsync();
        }

        public async Task<Product?> GetByIdAsync(object id)
        {
            return await _context.Products.FindAsync((int)id);
        }

        public async Task<IEnumerable<Product>> FindAsync(Expression<Func<Product, bool>> predicate)
        {
            return await _context.Products.Where(predicate).AsNoTracking().ToListAsync();
        }

        public async Task AddAsync(Product entity)
        {
            await _context.Products.AddAsync(entity);
        }

        public void Update(Product entity)
        {
            _context.Products.Update(entity);
        }

        public void Delete(Product entity)
        {
            _context.Products.Remove(entity);
        }
    }
}
