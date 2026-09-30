using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class CustomersController : ControllerBase
    {
        private readonly CustomerService _service;

        public CustomersController(CustomerService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<Customer>>> GetAll(
            [FromQuery] PaginationQuery pagination)
        {
            var result =
                await _service.GetAllAsync(pagination);

            return Ok(result);
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<Customer>> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Id должен быть больше 0."
                });
            }

            var customer =
                await _service.GetByIdAsync(id);

            if (customer == null)
            {
                return NotFound(new
                {
                    message = "Клиент не найден."
                });
            }

            return Ok(customer);
        }

        [HttpPost]
        public async Task<ActionResult<Customer>> Create(
            CustomerRequest request)
        {
            var customer =
                await _service.CreateAsync(request);

            return CreatedAtAction(
                nameof(GetById),
                new { id = customer.Id },
                customer);
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Customer>> Update(
            int id,
            CustomerRequest request)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Id должен быть больше 0."
                });
            }

            var customer =
                await _service.UpdateAsync(
                    id,
                    request);

            if (customer == null)
            {
                return NotFound(new
                {
                    message = "Клиент не найден."
                });
            }

            return Ok(customer);
        }

        [HttpDelete("{id:int}")]
        public async Task<IActionResult> Delete(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Id должен быть больше 0."
                });
            }

            var deleted =
                await _service.DeleteAsync(id);

            if (!deleted)
            {
                return NotFound(new
                {
                    message = "Клиент не найден."
                });
            }

            return NoContent();
        }
    }
}
