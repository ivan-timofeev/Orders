using System.Text;
using System.Text.Json;
using Orders.Models.DataTransferObjects;
using RabbitMQ.Client;

namespace Orders.Services;

public interface IItemsMicroserviceApiClient
{
    void MakeRequestToReserveItems(ReserveItemsRequest request);
}

// RabbitMQ Api Client
public sealed class ItemsMicroserviceApiClient : IItemsMicroserviceApiClient, IDisposable
{
    private readonly IConnection _connection;
    private readonly IModel _model;

    public ItemsMicroserviceApiClient(IConfiguration configuration)
    {
        var factory = new ConnectionFactory
        {
            HostName = configuration["RabbitMqHostName"],
            AutomaticRecoveryEnabled = true
        };

        _connection = factory.CreateConnection();
        _model = _connection.CreateModel();

        _model.QueueDeclare(queue: "ReserveItemsRequest",
            durable: true,
            exclusive: false,
            autoDelete: false,
            arguments: null);
    }

    public void MakeRequestToReserveItems(ReserveItemsRequest request)
    {
        var json = JsonSerializer.Serialize(request);
        var body = Encoding.UTF8.GetBytes(json);

        _model.BasicPublish(exchange: "",
            routingKey: "ReserveItemsRequest",
            basicProperties: null,
            body: body);
    }

    public void Dispose()
    {
        _connection.Dispose();
        _model.Dispose();
    }
}
