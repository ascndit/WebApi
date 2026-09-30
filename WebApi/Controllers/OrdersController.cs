using Microsoft.AspNetCore.Mvc;
using WebApi.Dtos;
using WebApi.Models;
using WebApi.Services;

namespace WebApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class OrdersController : ControllerBase
    {
        private readonly OrderService _service;

        public OrdersController(OrderService service)
        {
            _service = service;
        }

        [HttpGet]
        public async Task<ActionResult<PagedResult<OrderResponse>>> GetAll(
            [FromQuery] OrderQuery query)
        {
            try
            {
                var result =
                    await _service.GetAllAsync(query);

                return Ok(result);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpGet("{id:int}")]
        public async Task<ActionResult<OrderResponse>> GetById(int id)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Id должен быть больше 0."
                });
            }

            var order =
                await _service.GetByIdAsync(id);

            if (order == null)
            {
                return NotFound(new
                {
                    message = "Заказ не найден."
                });
            }

            return Ok(order);
        }

        [HttpPost]
        public async Task<ActionResult<Order>> Create(
            OrderRequest request)
        {
            try
            {
                var order =
                    await _service.CreateAsync(request);

                return CreatedAtAction(
                    nameof(GetById),
                    new { id = order.Id },
                    order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
        }

        [HttpPut("{id:int}")]
        public async Task<ActionResult<Order>> Update(
            int id,
            OrderRequest request)
        {
            if (id <= 0)
            {
                return BadRequest(new
                {
                    message = "Id должен быть больше 0."
                });
            }

            try
            {
                var order =
                    await _service.UpdateAsync(
                        id,
                        request);

                if (order == null)
                {
                    return NotFound(new
                    {
                        message = "Заказ не найден."
                    });
                }

                return Ok(order);
            }
            catch (ArgumentException ex)
            {
                return BadRequest(new
                {
                    message = ex.Message
                });
            }
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
                    message = "Заказ не найден."
                });
            }

            return NoContent();
        }
    }
}
