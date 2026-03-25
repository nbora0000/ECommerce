using CatalogApi.Models;
using CatalogApi.Repositories;
using Microsoft.EntityFrameworkCore;

namespace CatalogApi.Services
{
    /// <summary>
    /// Product service — business logic layer between controller and repository.
    /// </summary>
    public class ProductService : IProductService
    {
        private readonly IProductRepository _repository;
        private readonly CatalogApi.Data.CatalogDbContext _unitOfWork;

        public ProductService(IProductRepository repository, CatalogApi.Data.CatalogDbContext unitOfWork)
        {
            _repository = repository;
            _unitOfWork = unitOfWork;
        }

        public async Task<IEnumerable<Product>> GetAllProductsAsync(string? category)
        {
            return await _repository.GetAllProductsAsync(category);
        }

        public async Task<Product?> GetProductByIdAsync(int id)
        {
            return await _repository.GetProductByIdAsync(id);
        }

        public async Task<Product> CreateProductAsync(Product product)
        {
            await _repository.AddAsync(product);
            await _unitOfWork.SaveChangesAsync();
            return product;
        }

        public async Task<bool> UpdateProductAsync(int id, Product product)
        {
            if (id != product.Id) return false;

            try
            {
                _repository.Update(product);
                await _unitOfWork.SaveChangesAsync();
                return true;
            }
            catch (DbUpdateConcurrencyException)
            {
                if (!_repository.ProductExists(id))
                    return false;
                throw;
            }
        }

        public async Task<bool> DeleteProductAsync(int id)
        {
            var product = await _repository.GetByIdAsync(id);
            if (product is null) return false;

            _repository.Delete(product);
            await _unitOfWork.SaveChangesAsync();
            return true;
        }
    }
}
