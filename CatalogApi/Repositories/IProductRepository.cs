using CatalogApi.Models;
using SharedLibrary.Interfaces;

namespace CatalogApi.Repositories
{
    /// <summary>
    /// Product-specific repository with stored-procedure-backed query methods.
    /// </summary>
    public interface IProductRepository : IRepository<Product>
    {
        Task<IEnumerable<Product>> GetAllProductsAsync(string? category);
        Task<Product?> GetProductByIdAsync(int id);
        Task<IEnumerable<Product>> GetProductsByCategoryAsync(string category);
        Task<IEnumerable<Product>> SearchProductsAsync(string searchTerm);
        bool ProductExists(int id);
    }
}
