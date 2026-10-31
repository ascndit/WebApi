using Microsoft.Data.Sqlite;
using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Dtos;
using WebApi.Exceptions;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Tests;

public sealed class ServiceTests : IDisposable
{
    private readonly SqliteConnection _connection;
    private readonly AppDbContext _db;
    private readonly OrderService _orders;
    private readonly CustomerService _customers;

    public ServiceTests()
    {
        _connection = new SqliteConnection("Data Source=:memory:");
        _connection.Open();

        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseSqlite(_connection)
            .Options;

        _db = new AppDbContext(options);
        _db.Database.EnsureCreated();
        _orders = new OrderService(_db);
        _customers = new CustomerService(_db);
    }

    [Fact]
    public async Task GetOrders_FiltersPaidAndPaginates()
    {
        var customer = await _customers.CreateAsync(new CustomerRequest
        {
            Name = "Анна",
            Email = "anna@example.com"
        });

        await _orders.CreateAsync(Order(customer.Id, 100, true));
        await _orders.CreateAsync(Order(customer.Id, 200, true));
        await _orders.CreateAsync(Order(customer.Id, 300, false));

        var page = await _orders.GetAllAsync(new OrderQuery
        {
            IsPaid = true,
            Page = 1,
            PageSize = 1
        });

        Assert.Equal(2, page.TotalCount);
        Assert.Equal(2, page.TotalPages);
        Assert.Single(page.Items);
        Assert.All(page.Items, item => Assert.True(item.IsPaid));
    }

    [Fact]
    public async Task GetOrders_RejectsNegativeMinAmount()
    {
        var error = await Assert.ThrowsAsync<ArgumentException>(() =>
            _orders.GetAllAsync(new OrderQuery { MinAmount = -1 }));

        Assert.Contains("minAmount", error.Message);
    }

    [Fact]
    public async Task CreateOrder_RejectsUnknownCustomer()
    {
        var error = await Assert.ThrowsAsync<ArgumentException>(() =>
            _orders.CreateAsync(Order(customerId: 999, amount: 10, isPaid: false)));

        Assert.Contains("CustomerId", error.Message);
    }

    [Fact]
    public async Task DeleteCustomer_RejectsWhenOrdersExist()
    {
        var customer = await _customers.CreateAsync(new CustomerRequest
        {
            Name = "Илья",
            Email = "ilya@example.com"
        });
        await _orders.CreateAsync(Order(customer.Id, 50, false));

        var error = await Assert.ThrowsAsync<ConflictException>(() =>
            _customers.DeleteAsync(customer.Id));

        Assert.Contains("заказы", error.Message);
    }

    public void Dispose()
    {
        _db.Dispose();
        _connection.Dispose();
    }

    private static OrderRequest Order(int customerId, int amount, bool isPaid)
    {
        return new OrderRequest
        {
            CustomerId = customerId,
            Description = "Заказ",
            Amount = amount,
            IsPaid = isPaid
        };
    }
}
