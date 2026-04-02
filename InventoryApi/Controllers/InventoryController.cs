using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using InventoryApi.Data;
using InventoryApi.Models;
using SharedLibrary.Events;
using MediatR;

namespace InventoryApi.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class InventoryController : ControllerBase
    {
        private readonly IMediator _mediator;
        private readonly ILogger<InventoryController> _logger;

        public InventoryController(IMediator mediator, ILogger<InventoryController> logger)
        {
            _mediator = mediator;
            _logger = logger;
        }

        [HttpPost("reserve")]
        public async Task<IActionResult> Reserve([FromBody] ReserveRequest request)
        {
            var success = await _mediator.Send(new Features.Inventory.Commands.ReserveInventoryCommand(request.ProductId, request.OrderId, request.Quantity));
            if (!success) return BadRequest("Insufficient stock");

            var ev = new SharedLibrary.Events.InventoryReservedEvent
            {
                ReservationId = Guid.NewGuid(),
                OrderId = request.OrderId,
                ProductId = request.ProductId,
                Quantity = request.Quantity
            };

            _logger.LogInformation("Inventory reserved: {Event}", ev);

            return Ok(ev);
        }
    }

    public class ReserveRequest
    {
        public Guid OrderId { get; set; }
        public Guid ProductId { get; set; }
        public int Quantity { get; set; }
    }
}
