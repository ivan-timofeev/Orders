using System.Data;
using Microsoft.EntityFrameworkCore;
using Orders.Data;
using Orders.Models.DataTransferObjects;
using Orders.Models.DomainModels;

namespace Orders.Services;

public interface IOrdersManagementService
{
    IReadOnlyCollection<Order> GetAllOrders();
    Guid CreateOrder(CreateOrderDto createOrderDto);
    void UpdateOrderStatus(Guid orderId, OrderStatusEnum newStatus, string? details = null);
    void MakeRequestToReserveOrderItems(Guid orderId);
}

public sealed class OrdersManagementService : IOrdersManagementService
{
    private readonly IDbContextFactory<OrdersDbContext> _dbContextFactory;
    private readonly IItemsMicroserviceApiClient _itemsMicroserviceApiClient;
    private readonly ILogger<OrdersManagementService> _logger;

    public OrdersManagementService(
        IDbContextFactory<OrdersDbContext> dbContextFactory,
        IItemsMicroserviceApiClient itemsMicroserviceApiClient,
        ILogger<OrdersManagementService> logger)
    {
        _dbContextFactory = dbContextFactory;
        _itemsMicroserviceApiClient = itemsMicroserviceApiClient;
        _logger = logger;
    }

    public IReadOnlyCollection<Order> GetAllOrders()
    {
        _logger.LogInformation("api/orders/: Create db context");
        using var dbContext = _dbContextFactory.CreateDbContext();
        _logger.LogInformation("api/orders/: Db context created");
        
        
        _logger.LogInformation("api/orders/: Select orders");
        var result = dbContext
            .Orders
            .Include(o => o.RequestedItems)
            .Include(o => o.OrderStatusHistory)
            .ToArray();
        _logger.LogInformation("api/orders/: Orders selected");

        return result;
    }
    
    public Guid CreateOrder(CreateOrderDto createOrderDto)
    {
        using var dbContext = _dbContextFactory.CreateDbContext();
        using var transaction = dbContext.Database.BeginTransaction();
        var isTransactionCommitted = false;

        try
        {
            var requestedItems = createOrderDto
                .RequestedItems
                .Select(
                    i => new RequestedItem
                    {
                        Quantity = i.RequestedQuantity,
                        ItemId = i.ItemId
                    })
                .ToArray();

            var orderHistory = new List<OrderStatusHistoryItem>
            {
                new OrderStatusHistoryItem
                {
                    OrderStatus = OrderStatusEnum.Created,
                    EnterToStatusDateTimeUtc = DateTime.UtcNow,
                    Details = "Order created."
                }
            };

            var newOrder = new Order
            {
                CustomerId = createOrderDto.CustomerId,
                RequestedItems = requestedItems,
                OrderStatusHistory = orderHistory
            };

            dbContext.Orders.Add(newOrder);
            dbContext.SaveChanges();

            transaction.Commit();
            isTransactionCommitted = true;

            return newOrder.Id;
        }
        finally
        {
            if (!isTransactionCommitted)
                transaction.Rollback();
        }
    }

    public void UpdateOrderStatus(Guid orderId, OrderStatusEnum newStatus, string? details)
    {
        var dbContext = _dbContextFactory.CreateDbContext();
        using var transaction = dbContext.Database.BeginTransaction(IsolationLevel.Serializable);

        var order = dbContext
            .Orders
            .Include(o => o.OrderStatusHistory)
            .Include(o => o.RequestedItems)
            .Where(o => o.Id == orderId)
            .Single();

        order.OrderStatusHistory.Add(
            new OrderStatusHistoryItem
            {
                OrderStatus = newStatus,
                EnterToStatusDateTimeUtc = DateTime.UtcNow,
                Details = details
            });

        dbContext.SaveChanges();
        transaction.Commit();
    }
    
    public void MakeRequestToReserveOrderItems(Guid orderId)
    {
        var dbContext = _dbContextFactory.CreateDbContext();
        using var transaction = dbContext.Database.BeginTransaction(IsolationLevel.Serializable);

        var order = dbContext
            .Orders
            .Include(o => o.OrderStatusHistory)
            .Include(o => o.RequestedItems)
            .Where(o => o.Id == orderId)
            .Single();

        var orderRequest = new ReserveItemsRequest(
            TransactionalId: Guid.NewGuid(),
            OrderId: order.Id,
            RequestedItems: order.RequestedItems.Select(i => new RequestedItemDto(i.ItemId, i.Quantity)));
        
        order.OrderStatusHistory.Add(
            new OrderStatusHistoryItem
            {
                OrderStatus = OrderStatusEnum.Processing,
                EnterToStatusDateTimeUtc = DateTime.UtcNow,
                Details = "Sent reserve request to Items Microservice."
            });

        _itemsMicroserviceApiClient.MakeRequestToReserveItems(orderRequest);

        dbContext.SaveChanges();
        transaction.Commit();
    }
}
