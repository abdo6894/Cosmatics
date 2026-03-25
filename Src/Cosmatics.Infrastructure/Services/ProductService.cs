using Cosmatics.Models;
using Cosmatics.Application.DTOs;
using Cosmatics.Infrastructure.Persistense.Data;
using Microsoft.Extensions.Caching.Memory;
using Cosmatics.Application.Common;


namespace Cosmatics.Infrastructure.Services;

public class ProductService : IProductService
{
    private readonly IRepository<Product> _productRepo;
    //private readonly IMemoryCache _cache;
    private readonly ICacheService _cacheService;
    public ProductService(IRepository<Product> productRepo, ICacheService cache)
    {
        _productRepo = productRepo;
        _cacheService = cache;
    }

    public async Task<IEnumerable<Product>> GetAllProductsAsync()
    {
        var key = "Get_All:Products";
        var cacheProducts = await _cacheService.GetDataAsync<IEnumerable<Product>>(key);

        if (cacheProducts is not null)
        {
            Console.WriteLine("Cache Visited");
            return cacheProducts;


        }

        var entities = await _productRepo.GetAllAsync();

        if (entities.Any()) 
            await _cacheService.SetDataAsync(key, entities, TimeSpan.FromMinutes(10));
        Console.WriteLine("DB Visited");

        return entities; 
    }

    public async Task<Product?> GetProductByIdAsync(int id)
    {
        var key = $"Get_Product:{id}";

        var product = await _cacheService.GetDataAsync<Product>(key);
        
            if (product is not null)
            {
                    Console.WriteLine("Cache Visited");
                    return product;
            }
           
                Console.WriteLine("DB Vistied");
                var entitey = await _productRepo.GetByIdAsync(id);
                await _cacheService.SetDataAsync(key, entitey, TimeSpan.FromMinutes(10));
                return entitey;

    }
        
    public async Task<Product> CreateProductAsync(ProductCreateDto dto)
    {
        var product = new Product
        {
            Name = dto.Name,
            Description = dto.Description,
            Price = dto.Price,
            Stock = dto.Stock,
            ImageUrl = dto.ImageUrl
        };
        await _productRepo.AddAsync(product);
         await _cacheService.RemoveDataAsync("Get_All:Products");
        await _cacheService.RemoveDataAsync($"Get_Product:{product.Id}");

        return product;
    }

    public async Task UpdateProductAsync(int id, ProductUpdateDto dto)
    {
        var product = await _productRepo.GetByIdAsync(id);
        if (product != null)
        {
            product.Name = dto.Name;
            product.Description = dto.Description;
            product.Price = dto.Price;
            product.Stock = dto.Stock;
            product.ImageUrl = dto.ImageUrl;
            await _productRepo.UpdateAsync(product);

            await _cacheService.RemoveDataAsync("Get_All:Products");
            await _cacheService.RemoveDataAsync($"Get_Product:{product.Id}");
        }

    }

    public async Task DeleteProductAsync(int id)
    {
        var product = await _productRepo.GetByIdAsync(id);
        if (product != null)
        {
            await _productRepo.DeleteAsync(product);

            await _cacheService.RemoveDataAsync("Get_All:Products");
            await _cacheService.RemoveDataAsync($"Get_Product:{product.Id}");
        }


    }

}
