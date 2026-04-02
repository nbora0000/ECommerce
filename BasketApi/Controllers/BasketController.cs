using BasketApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace BasketApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BasketController : ControllerBase
{
    private readonly IMediator _mediator;

    public BasketController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet("{customerId}")]
    public async Task<ActionResult<ShoppingCart>> GetBasket(string customerId)
    {
        var basket = await _mediator.Send(new Features.Basket.Queries.GetBasketQuery(customerId));
        return Ok(basket);
    }

    [HttpPost]
    public async Task<ActionResult<ShoppingCart>> UpdateBasket(ShoppingCart basket)
    {
        var updated = await _mediator.Send(new Features.Basket.Commands.UpdateBasketCommand(basket));
        return CreatedAtAction(nameof(GetBasket), new { customerId = updated.CustomerId }, updated);
    }

    [HttpDelete("{customerId}")]
    public async Task<IActionResult> DeleteBasket(string customerId)
    {
        var deleted = await _mediator.Send(new Features.Basket.Commands.DeleteBasketCommand(customerId));
        if (!deleted) return NotFound();
        return NoContent();
    }
}
