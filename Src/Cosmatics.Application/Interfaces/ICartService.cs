using Cosmatics.Application.DTOs;

namespace Cosmatics.Infrastructure.Services;

public interface ICartService
{
    Task AddToCartAsync(int userId, int productId, int quantity);
    Task RemoveFromCartAsync(int userId, int productId);
    Task<CartDto> GetCartAsync(int userId);
    Task ClearCartAsync(int userId);
    Task<bool> UpdateCartItemQuantityAsync(int userId, int productId, int quantity);
}
