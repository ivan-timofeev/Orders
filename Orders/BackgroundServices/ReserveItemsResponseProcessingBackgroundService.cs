using System.Text;
using System.Text.Json;
using Orders.Models.DataTransferObjects;
using Orders.Services;
using RabbitMQ.Client;
using RabbitMQ.Client.Events;

namespace Orders.BackgroundServices;

public sealed class ReserveItemsResponseProcessingBackgroundService : BackgroundService
{
    private readonly ILogger<ReserveItemsResponseProcessingBackgroundService> _logger;
    private readonly IReserveItemsResponseProcessor _reserveItemsResponseProcessor;
    private readonly IConfiguration _configuration;
    private IConnection? _connection;
    private IModel? _channel;

    public ReserveItemsResponseProcessingBackgroundService(
        ILogger<ReserveItemsResponseProcessingBackgroundService> logger,
        IReserveItemsResponseProcessor reserveItemsResponseProcessor,
        IConfiguration configuration)
    {
        _logger = logger;
        _reserveItemsResponseProcessor = reserveItemsResponseProcessor;
        _configuration = configuration;
    }

    protected override Task ExecuteAsync(CancellationToken cancellationToken)
    {
        Task.Run(() =>
        {
            InitializeRabbitMq();
            
            var consumer = new EventingBasicConsumer(_channel);
            consumer.Received += (ch, eventArgs) =>
            {
                var json = Encoding.UTF8.GetString(eventArgs.Body.ToArray());
                var reserveItemsResponse = JsonSerializer.Deserialize<ReserveItemsResponse>(json)
                                           ?? throw new InvalidOperationException("Json must be of type CreateOrderRequest");

                _logger.LogInformation(
                    "Started processing of ReserveItemsResponse. TransactionalId: {T}",
                    reserveItemsResponse.TransactionalId);

                _reserveItemsResponseProcessor.ProcessReserveItemsResponse(reserveItemsResponse);

                _logger.LogInformation(
                    "ReserveItemsResponse processed. TransactionalId: {T}",
                    reserveItemsResponse.TransactionalId);

                _channel!.BasicAck(eventArgs.DeliveryTag, false);
            };

            _channel.BasicConsume("ReserveItemsResponse", false, consumer);
        }, cancellationToken);

        return Task.CompletedTask;
    }

    private void InitializeRabbitMq()
    {
        var factory = new ConnectionFactory
        {
            HostName = _configuration["RabbitMqHostName"],
            AutomaticRecoveryEnabled = true
        };

        while (_channel is null)
        {
            try
            {
                _connection = factory.CreateConnection();
                _channel = _connection.CreateModel();

                _channel.QueueDeclare(
                    queue: "ReserveItemsResponse",
                    durable: true,
                    exclusive: false,
                    autoDelete: false,
                    arguments: null);
            }
            catch (Exception ex)
            {
                _logger.LogError(
                    "Initialization error. Retrying after 120s\nMessage: {M}",
                    ex.Message);

                Thread.Sleep(120_000);
            }
        }
    }

    public override void Dispose()
    {
        _channel?.Close();
        _connection?.Close();
        base.Dispose();
    }
}
