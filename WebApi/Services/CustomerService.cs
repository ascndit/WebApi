using WebApi.Data;
using WebApi.Dtos;
using WebApi.Exceptions;
using WebApi.Models;
using Microsoft.EntityFrameworkCore;

namespace WebApi.Services
{
    public class CustomerService
    {
        private readonly AppDbContext _context;

        public CustomerService(AppDbContext context)
        {
            _context = context;
        }

        public async Task<PagedResult<Customer>> GetAllAsync(
            PaginationQuery pagination)
        {
            var query = _context.Customers
                 .OrderBy(c => c.Id);
            var totalCount = await query.CountAsync();
            var items = await query
                .Skip((pagination.Page - 1) * pagination.PageSize)
                .Take(pagination.PageSize)
                .ToListAsync();
            var totalPages =
                (int)Math.Ceiling(
                    totalCount / (double)pagination.PageSize);
            return new PagedResult<Customer>
            {
                Page = pagination.Page,
                PageSize = pagination.PageSize,
                TotalCount = totalCount,
                TotalPages = totalPages,
                Items = items
            };
        }

        public async Task<Customer?> GetByIdAsync(int id)
        {
            return await _context.Customers
                .FirstOrDefaultAsync(c => c.Id == id);
        }

        public async Task<Customer> CreateAsync(
            CustomerRequest request)
        {
            var customer = new Customer
            {
                Name = request.Name.Trim(),
                Email = request.Email.Trim()
            };
            await _context.Customers.AddAsync(customer);
            await _context.SaveChangesAsync();
            return customer;
        }

        public async Task<Customer?> UpdateAsync(
            int id,
            CustomerRequest request)
        {
            var existing =
                await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == id);
            if (existing == null)
            {
                return null;
            }
            existing.Name = request.Name.Trim();
            existing.Email = request.Email.Trim();
            await _context.SaveChangesAsync();
            return existing;
        }

        public async Task<bool> DeleteAsync(int id)
        {
            var existing =
                await _context.Customers
                    .FirstOrDefaultAsync(c => c.Id == id);

            if (existing == null)
            {
                return false;
            }

            var hasOrders = await _context.Orders
                .AnyAsync(order => order.CustomerId == id);

            if (hasOrders)
            {
                throw new ConflictException(
                    "Нельзя удалить клиента, у которого есть заказы.");
            }

            _context.Customers.Remove(existing);

            await _context.SaveChangesAsync();

            return true;
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
