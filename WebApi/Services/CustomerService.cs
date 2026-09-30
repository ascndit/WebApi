using WebApi.Dtos;
using WebApi.Exceptions;
using WebApi.Models;
using WebApi.Repositories;

namespace WebApi.Services
{
    public class CustomerService
    {
        private readonly JsonFileRepository<Customer> _customerRepository;
        private readonly JsonFileRepository<Order> _orderRepository;

        public CustomerService(
            JsonFileRepository<Customer> customerRepository,
            JsonFileRepository<Order> orderRepository)
        {
            _customerRepository = customerRepository;
            _orderRepository = orderRepository;
        }

        public async Task<PagedResult<Customer>> GetAllAsync(
            PaginationQuery pagination)
        {
            var customers =
                await _customerRepository.GetAllAsync();

            var ordered = customers
                .OrderBy(c => c.Id)
                .ToList();

            return CreatePagedResult(
                ordered,
                pagination);
        }

        public Task<Customer?> GetByIdAsync(int id)
        {
            return _customerRepository.GetByIdAsync(id);
        }

        public async Task<Customer> CreateAsync(
            CustomerRequest request)
        {
            return await _customerRepository.AddAsync(
                id => new Customer
                {
                    Id = id,
                    Name = request.Name.Trim(),
                    Email = request.Email.Trim()
                });
        }

        public async Task<Customer?> UpdateAsync(
            int id,
            CustomerRequest request)
        {
            var existing =
                await _customerRepository.GetByIdAsync(id);

            if (existing == null)
            {
                return null;
            }

            var customer = new Customer
            {
                Id = id,
                Name = request.Name.Trim(),
                Email = request.Email.Trim()
            };

            await _customerRepository.UpdateAsync(
                id,
                customer);

            return customer;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing =
                await _customerRepository.GetByIdAsync(id);

            if (existing == null)
            {
                return false;
            }

            var orders =
                await _orderRepository.GetAllAsync();

            var hasOrders = orders.Any(
                order => order.CustomerId == id);

            if (hasOrders)
            {
                throw new ConflictException(
                    "Нельзя удалить клиента, у которого есть заказы.");
            }

            return await _customerRepository.DeleteAsync(id);
        }

        private static PagedResult<Customer> CreatePagedResult(
            List<Customer> items,
            PaginationQuery pagination)
        {
            var totalCount = items.Count;

            var resultItems = items
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToList();

            var totalPages =
                (int)Math.Ceiling(
                    totalCount / (double)pagination.PageSize);

            return new PagedResult<Customer>
            {
                Page = pagination.Page,
                PageSize = pagination.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = resultItems
            };
        }
    }
}
