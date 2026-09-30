using WebApi.Middleware;
using WebApi.Models;
using WebApi.Repositories;
using WebApi.Services;

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddControllers();
var dataDirectory = Path.Combine(
    builder.Environment.ContentRootPath,
    "Data");

var customersFile = Path.Combine(
    dataDirectory,
    "customers.json");

var ordersFile = Path.Combine(
    dataDirectory,
    "orders.json");


builder.Services.AddSingleton(
    new JsonFileRepository<Customer>(
        customersFile,
        customer => customer.Id));

builder.Services.AddSingleton(
    new JsonFileRepository<Order>(
        ordersFile,
        order => order.Id));

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();


builder.Services.AddScoped<CustomerService>();
builder.Services.AddScoped<OrderService>();

var app = builder.Build();

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "API v1");
    c.RoutePrefix = "swagger";
});

app.UseMiddleware<ExceptionHandlingMiddleware>();

var customerRepository =
    app.Services
        .GetRequiredService<JsonFileRepository<Customer>>();

var orderRepository =
    app.Services
        .GetRequiredService<JsonFileRepository<Order>>();

await customerRepository.InitializeAsync();
await orderRepository.InitializeAsync();


app.MapControllers();

app.Run();
