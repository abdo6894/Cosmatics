using Cosmatics.Models;
using Cosmatics.Application.DTOs;

namespace Cosmatics.Infrastructure.Services;

public interface IOrderService
{
    Task<Order> PlaceOrderAsync(int userId, CreateOrderDto dto);
    Task<IEnumerable<Order>> GetUserOrdersAsync(int userId);
    Task<Order?> GetOrderByIdAsync(int orderId);
}
