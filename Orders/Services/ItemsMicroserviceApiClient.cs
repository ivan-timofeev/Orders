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
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _model;

    public ItemsMicroserviceApiClient(IConfiguration configuration)
    {
        _configuration = configuration;
    }

    public void MakeRequestToReserveItems(ReserveItemsRequest request)
    {
        if (_model is null || _connection is null)
            InitializeRabbitMq();
        
        var json = JsonSerializer.Serialize(request);
        var body = Encoding.UTF8.GetBytes(json);

        _model.BasicPublish(exchange: "",
            routingKey: "ReserveItemsRequest",
            basicProperties: null,
            body: body);
    }

    public void Dispose()
    {
        _connection?.Dispose();
        _model?.Dispose();
    }

    private void InitializeRabbitMq()
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMqHostName"],
            UserName = "guest",
            Port = int.Parse(_configuration["RabbitMqPort"]!),
            Password = _configuration["RabbitMqPassword"],
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
}
