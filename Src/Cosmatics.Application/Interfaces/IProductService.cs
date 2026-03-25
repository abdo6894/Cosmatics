using Cosmatics.Models;
using Cosmatics.Application.DTOs;

namespace Cosmatics.Infrastructure.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAllProductsAsync();
    Task<Product?> GetProductByIdAsync(int id);
    Task<Product> CreateProductAsync(ProductCreateDto dto);
    Task UpdateProductAsync(int id, ProductUpdateDto dto);
    Task DeleteProductAsync(int id);
}
