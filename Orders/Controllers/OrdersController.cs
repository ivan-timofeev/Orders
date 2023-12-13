using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.EntityFrameworkCore;
using Orders.Data;
using Orders.Models.DataTransferObjects;
using Orders.Services;

namespace Orders.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly ILogger<OrdersController> _logger;
    private readonly IOrdersManagementService _ordersManagementService;
    private readonly IDbContextFactory<OrdersDbContext> _dbContextFactory;

    public OrdersController(
        ILogger<OrdersController> logger,
        IOrdersManagementService ordersManagementService,
        IDbContextFactory<OrdersDbContext> dbContextFactory)
    {
        _logger = logger;
        _ordersManagementService = ordersManagementService;
        _dbContextFactory = dbContextFactory;
    }

    // GET: api/orders/
    [HttpGet]
    public IActionResult GetAll()
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        var orders = dbContext
            .Orders
            .Include(o => o.RequestedItems)
            .Include(o => o.OrderStatusHistory)
            .Select(
                o => new OrderDto
                {
                    Id = o.Id,
                    CustomerId = o.CustomerId,
                    OrderStatusHistory = o.OrderStatusHistory.Select(
                        h => new OrderStatusHistoryItemDto
                        {
                            OrderStatus = h.OrderStatus,
                            EnterToStatusDateTimeUtc = h.EnterToStatusDateTimeUtc,
                            Details = h.Details
                        }),
                    RequestedItems = o.RequestedItems.Select(
                        i => new RequestedItemDto(i.ItemId, i.Quantity))
                })
            .ToList();

        return Ok(orders);
    }

    // POST: api/orders/
    [HttpPost]
    public IActionResult CreateOrder(
        [FromBody, BindRequired] CreateOrderDto createOrderDto)
    {
        try
        {
            var orderId = _ordersManagementService.CreateOrder(createOrderDto);
            var response = new { Status = "Order created.", OrderId = orderId };

            return Ok(response);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
