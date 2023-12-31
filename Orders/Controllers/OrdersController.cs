using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Orders.Models.DataTransferObjects;
using Orders.Services;

namespace Orders.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OrdersController : ControllerBase
{
    private readonly IOrdersManagementService _ordersManagementService;
    private readonly ILogger<OrdersController> _logger;

    public OrdersController(
        IOrdersManagementService ordersManagementService,
        ILogger<OrdersController> logger)
    {
        _ordersManagementService = ordersManagementService;
        _logger = logger;
    }

    // GET: api/orders/
    [HttpGet]
    public IActionResult GetAll()
    {
        _logger.LogInformation("api/orders/: GetAll()");
        var orders = _ordersManagementService
            .GetAllOrders()
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

            return Ok(orderId);
        }
        catch (Exception ex)
        {
            return StatusCode(StatusCodes.Status500InternalServerError, ex.Message);
        }
    }
}
