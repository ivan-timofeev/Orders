using Microsoft.EntityFrameworkCore;
using Orders.Models.DomainModels;

namespace Orders.Data;

public sealed class OrdersDbContext : DbContext
{
    public DbSet<Order> Orders { get; set; }
    public DbSet<OrderStatusHistoryItem> OrderStatusHistoryItems { get; set; }

    public OrdersDbContext(DbContextOptions<OrdersDbContext> options) : base(options)
    {
        Orders = Set<Order>();
        OrderStatusHistoryItems = Set<OrderStatusHistoryItem>();

        Database.EnsureCreated();
    }
}
