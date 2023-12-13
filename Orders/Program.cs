using Microsoft.EntityFrameworkCore;
using Orders.BackgroundServices;
using Orders.Data;
using Orders.Services;

var builder = WebApplication.CreateBuilder(args);

// Add services to the container.

builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();

var connectionString = builder.Configuration["OrdersSqlConnectionString"];
builder.Services.AddDbContextFactory<OrdersDbContext>(
    options => options.UseSqlServer(connectionString));

builder.Services.AddDbContextFactory<OrdersDbContext>();
builder.Services.AddTransient<IOrdersManagementService, OrdersManagementService>();
builder.Services.AddSingleton<IItemsMicroserviceApiClient, ItemsMicroserviceApiClient>();
builder.Services.AddTransient<IReserveItemsResponseProcessor, ReserveItemsResponseProcessor>();

builder.Services.AddHostedService<StartProcessingOfCreatedOrdersBackgroundService>();
builder.Services.AddHostedService<ReserveItemsResponseProcessingBackgroundService>();


var app = builder.Build();
app.UseSwagger();
app.UseSwaggerUI();
app.MapControllers();

app.Run();