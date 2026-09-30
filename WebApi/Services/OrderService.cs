using WebApi.Dtos;
using WebApi.Models;
using WebApi.Repositories;

namespace WebApi.Services
{
    public class OrderService
    {
        private readonly JsonFileRepository<Order> _orderRepository;
        private readonly JsonFileRepository<Customer> _customerRepository;

        public OrderService(
            JsonFileRepository<Order> orderRepository,
            JsonFileRepository<Customer> customerRepository)
        {
            _orderRepository = orderRepository;
            _customerRepository = customerRepository;
        }

        public async Task<PagedResult<OrderResponse>> GetAllAsync(
            OrderQuery query)
        {
            ValidateQuery(query);

            var orders =
                await _orderRepository.GetAllAsync();

            var customers =
                await _customerRepository.GetAllAsync();

            var result =
                from order in orders
                join customer in customers
                    on order.CustomerId equals customer.Id
                select new OrderResponse
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
                };

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

            var list = result
                .OrderByDescending(order => order.Id)
                .ToList();

            var totalCount = list.Count;

            var items = list
                .Skip((query.Page - 1) * query.PageSize)
                .Take(query.PageSize)
                .ToList();

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
            var orders =
                await _orderRepository.GetAllAsync();

            var customers =
                await _customerRepository.GetAllAsync();

            var result =
                from order in orders
                join customer in customers
                    on order.CustomerId equals customer.Id
                where order.Id == id
                select new OrderResponse
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
                };

            return result.FirstOrDefault();
        }

        public async Task<Order> CreateAsync(
            OrderRequest request)
        {
            var customer =
                await _customerRepository
                    .GetByIdAsync(request.CustomerId);

            if (customer == null)
            {
                throw new ArgumentException(
                    "Клиент с указанным CustomerId не существует.");
            }

            return await _orderRepository.AddAsync(
                id => new Order
                {
                    Id = id,
                    CustomerId = request.CustomerId,
                    Description = request.Description.Trim(),
                    Amount = request.Amount,
                    IsPaid = request.IsPaid,
                    CreatedAt = DateTime.UtcNow
                });
        }

        public async Task<Order?> UpdateAsync(
            int id,
            OrderRequest request)
        {
            var existing =
                await _orderRepository.GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            var customer =
                await _customerRepository
                    .GetByIdAsync(request.CustomerId);

            if (customer == null)
            {
                throw new ArgumentException(
                    "Клиент с указанным CustomerId не существует.");
            }

            var updated = new Order
            {
                Id = id,
                CustomerId = request.CustomerId,
                Description = request.Description.Trim(),
                Amount = request.Amount,
                IsPaid = request.IsPaid,
                CreatedAt = existing.CreatedAt
            };

            await _orderRepository.UpdateAsync(
                id,
                updated);

            return updated;
        }

        public Task<bool> DeleteAsync(int id)
        {
            return _orderRepository.DeleteAsync(id);
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
