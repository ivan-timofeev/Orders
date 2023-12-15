using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Orders.Controllers;
using Orders.Models.DataTransferObjects;
using Orders.Services;

namespace Orders.Tests;

[TestClass]
public class OrdersControllerTests
{
    [TestMethod]
    public void CreateOrder_HappyPath_ShouldReturn200AndGuidOfCreatedOrder()
    {
        // Arrange
        var requestedItem = new RequestedItemDto(Guid.NewGuid(), 10);
        var createOrderDto = new CreateOrderDto(
            Guid.NewGuid(),
            new[] { requestedItem });

        var ordersManagementServiceMock = new Mock<IOrdersManagementService>();
        var createdOrderId = Guid.NewGuid();

        ordersManagementServiceMock
            .Setup(s => s.CreateOrder(createOrderDto))
            .Returns(createdOrderId);

        var controller = new OrdersController(
            ordersManagementServiceMock.Object,
            Mock.Of<ILogger<OrdersController>>());

        // Act
        var result = controller.CreateOrder(createOrderDto);

        // Assert
        Assert.IsInstanceOfType(result, typeof(OkObjectResult));
        Assert.AreEqual(createdOrderId, ((OkObjectResult)result).Value);
    }
    
    
    [TestMethod]
    public void CreateOrder_ErrorThrows_ShouldReturn500()
    {
        // Arrange
        var createOrderDto = new CreateOrderDto(
            Guid.NewGuid(),
            new[] { new RequestedItemDto(Guid.NewGuid(), 10) });
        
        var controller = new OrdersController(
            null!,
            Mock.Of<ILogger<OrdersController>>());

        // Act
        var result = controller.CreateOrder(createOrderDto);

        // Assert
        Assert.IsInstanceOfType(result, typeof(ObjectResult));
        Assert.AreEqual(
            StatusCodes.Status500InternalServerError,
            ((ObjectResult)result).StatusCode);
    }
}
