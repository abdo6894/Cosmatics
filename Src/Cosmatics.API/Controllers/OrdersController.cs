using Cosmatics.Infrastructure.Services;
using Cosmatics.Application.DTOs;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace Cosmatics.Controllers;

[Route("api/[controller]")]
[ApiController]
[Authorize]
public class OrdersController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrdersController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    private int GetUserId() => int.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    [HttpPost]
    public async Task<IActionResult> PlaceOrder(CreateOrderDto dto)
    {
        try
        {
            var order = await _orderService.PlaceOrderAsync(GetUserId(), dto);
            return Ok(new { OrderId = order.Id, Status = order.Status, Total = order.TotalAmount });
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet]
    public async Task<IActionResult> GetMyOrders()
    {
        return Ok(await _orderService.GetUserOrdersAsync(GetUserId()));
    }
}
