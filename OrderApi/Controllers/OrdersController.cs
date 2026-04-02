using Microsoft.AspNetCore.Mvc;
using OrderApi.DTOs;
using OrderApi.Services;
using SharedLibrary.Enums;
using MediatR;

namespace OrderApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Produces("application/json")]
    public class OrdersController : ControllerBase
    {
        private readonly IMediator _mediator;

        public OrdersController(IMediator mediator)
        {
            _mediator = mediator;
        }

        /// <summary>Get all orders</summary>
        [HttpGet]
        [ProducesResponseType(typeof(IEnumerable<OrderResponseDto>), 200)]
        public async Task<IActionResult> GetAll([FromQuery] OrderStatus? status, [FromQuery] int page = 1, [FromQuery] int pageSize = 20)
        {
            var (orders, totalCount) = await _mediator.Send(new Features.Orders.Queries.GetAllOrdersQuery(status, page, pageSize));
            Response.Headers.Append("X-Total-Count", totalCount.ToString());
            return Ok(orders);
        }

        /// <summary>Get order by ID</summary>
        [HttpGet("{id:guid}")]
        [ProducesResponseType(typeof(OrderResponseDto), 200)]
        [ProducesResponseType(404)]
        public async Task<IActionResult> GetById(Guid id)
        {
            var order = await _mediator.Send(new Features.Orders.Queries.GetOrderByIdQuery(id));
            if (order is null) return NotFound(new { message = $"Order {id} not found." });
            return Ok(order);
        }

        /// <summary>Create a new order</summary>
        [HttpPost]
        [ProducesResponseType(typeof(OrderResponseDto), 201)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Create([FromBody] CreateOrderDto dto)
        {
            if (!ModelState.IsValid) return BadRequest(ModelState);
            if (!dto.Items.Any()) return BadRequest(new { message = "Order must have at least one item." });

            var order = await _mediator.Send(new Features.Orders.Commands.CreateOrderCommand(dto));
            return CreatedAtAction(nameof(GetById), new { id = order.Id }, order);
        }

        /// <summary>Update an order</summary>
        [HttpPut("{id:guid}")]
        [ProducesResponseType(typeof(OrderResponseDto), 200)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Update(Guid id, [FromBody] UpdateOrderDto dto)
        {
            try
            {
                var order = await _mediator.Send(new Features.Orders.Commands.UpdateOrderCommand(id, dto));
                if (order is null) return NotFound(new { message = $"Order {id} not found." });
                return Ok(order);
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }

        /// <summary>Delete an order</summary>
        [HttpDelete("{id:guid}")]
        [ProducesResponseType(204)]
        [ProducesResponseType(404)]
        [ProducesResponseType(400)]
        public async Task<IActionResult> Delete(Guid id)
        {
            try
            {
                var deleted = await _mediator.Send(new Features.Orders.Commands.DeleteOrderCommand(id));
                if (!deleted) return NotFound(new { message = $"Order {id} not found." });
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return BadRequest(new { message = ex.Message });
            }
        }
    }
}
