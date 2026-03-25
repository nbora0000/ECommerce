using BasketApi.Models;
using BasketApi.Services;
using Microsoft.AspNetCore.Mvc;

namespace BasketApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class BasketController : ControllerBase
{
    private readonly IBasketService _basketService;

    public BasketController(IBasketService basketService)
    {
        _basketService = basketService;
    }

    [HttpGet("{customerId}")]
    public async Task<ActionResult<ShoppingCart>> GetBasket(string customerId)
    {
        var basket = await _basketService.GetBasketAsync(customerId);
        return basket;
    }

    [HttpPost]
    public async Task<ActionResult<ShoppingCart>> UpdateBasket(ShoppingCart basket)
    {
        var updated = await _basketService.UpdateBasketAsync(basket);
        return CreatedAtAction(nameof(GetBasket), new { customerId = updated.CustomerId }, updated);
    }

    [HttpDelete("{customerId}")]
    public async Task<IActionResult> DeleteBasket(string customerId)
    {
        var deleted = await _basketService.DeleteBasketAsync(customerId);
        if (!deleted) return NotFound();

        return NoContent();
    }
}
