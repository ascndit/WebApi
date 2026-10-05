using Microsoft.EntityFrameworkCore;
using WebApi.Data;
using WebApi.Dtos;
using WebApi.Models;

namespace WebApi.Services
{
    public class OrderService
    {
        private readonly AppDbContext _context;

        public OrderService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<OrderResponse>> GetAllAsync(
            OrderQuery query)
        {
            ValidateQuery(query);

            var result = _context.Orders
                .Join(
                    _context.Customers,
                    order => order.CustomerId,
                    customer => customer.Id,
                    (order, customer) => new OrderResponse
                    {
                        Id = order.Id,
                        CustomerId = order.CustomerId,
                        Description = order.Description,
                        Amount = order.Amount,
                        IsPaid = order.IsPaid,
                        CreatedAt = order.CreatedAt,
                        Customer = new CustomerInfoDto
                        {
                            Id = customer.Id,
                            Name = customer.Name,
                            Email = customer.Email
                        }
                    });

            if (query.CustomerId.HasValue)
            {
                result = result.Where(
                    order =>
                        order.CustomerId ==
                        query.CustomerId.Value);
            }

            if (query.IsPaid.HasValue)
            {
                result = result.Where(
                    order =>
                        order.IsPaid ==
                        query.IsPaid.Value);
            }

            if (query.MinAmount.HasValue)
            {
                result = result.Where(
                    order =>
                        order.Amount >=
                        query.MinAmount.Value);
            }

            result = result
                .OrderByDescending(order => order.Id);

            var totalCount = await result.CountAsync();

            var items = await result
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToListAsync();

            var totalPages =
                (int)Math.Ceiling(
                    totalCount / (double)query.PageSize);

            return new PagedResult<OrderResponse>
            {
                Page = query.Page,
                PageSize = query.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };
        }

        public async Task<OrderResponse?> GetByIdAsync(int id)
        {
            var result = _context.Orders
                .Join(
                    _context.Customers,
                    order => order.CustomerId,
                    customer => customer.Id,
                    (order, customer) => new OrderResponse
                    {
                    Id = order.Id,
                    CustomerId = order.CustomerId,
                    Description = order.Description,
                    Amount = order.Amount,
                    IsPaid = order.IsPaid,
                    CreatedAt = order.CreatedAt,

                    Customer = new CustomerInfoDto
                    {
                        Id = customer.Id,
                        Name = customer.Name,
                        Email = customer.Email
                    }
                });

            return await result
                .Where(order => order.Id == id)
                .FirstOrDefaultAsync();
        }

        public async Task<Order> CreateAsync(
            OrderRequest request)
        {
            var customer = await _context.Customers
                .FirstOrDefaultAsync(customer => customer.Id == request.CustomerId);

            if (customer == null)
            {
                throw new ArgumentException(
                    "Клиент с указанным CustomerId не существует.");
            }

            var order = new Order
            {
                CustomerId = request.CustomerId,
                Description = request.Description.Trim(),
                Amount = request.Amount,
                IsPaid = request.IsPaid,
                CreatedAt = DateTime.UtcNow
            };
            await _context.Orders.AddAsync(order);
            await _context.SaveChangesAsync();
            return order;
        }

        public async Task<Order?> UpdateAsync(
            int id,
            OrderRequest request)
        {
            var existing =
                await _context.Orders
                    .FirstOrDefaultAsync(order => order.Id == id);

            if (existing == null)
            {
                return null;
            }

            var customer = await _context.Customers
                .FirstOrDefaultAsync(customer => customer.Id == request.CustomerId);

            if (customer == null)
            {
                throw new ArgumentException(
                    "Клиент с указанным CustomerId не существует.");
            }

                existing.CustomerId = request.CustomerId;
                existing.Description = request.Description.Trim();
                existing.Amount = request.Amount;
                existing.IsPaid = request.IsPaid;

            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing =
                await _context.Orders
                    .FirstOrDefaultAsync(order => order.Id == id);

            if (existing == null)
            {
                return false;
            }
            _context.Orders.Remove(existing);

            await _context.SaveChangesAsync();

            return true;
        }



        private static void ValidateQuery(OrderQuery query)
        {
            if (query.CustomerId.HasValue &&
                query.CustomerId.Value <= 0)
            {
                throw new ArgumentException(
                    "customerId должен быть больше 0.");
            }

            if (query.MinAmount.HasValue &&
                query.MinAmount.Value < 0)
            {
                throw new ArgumentException(
                    "minAmount не может быть отрицательным.");
            }
        }
    }
}
