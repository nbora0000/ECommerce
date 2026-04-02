using CatalogApi.Models;
using MediatR;
using Microsoft.AspNetCore.Mvc;

namespace CatalogApi.Controllers;

[ApiController]
[Route("api/[controller]")]
public class ProductsController : ControllerBase
{
    private readonly IMediator _mediator;

    public ProductsController(IMediator mediator)
    {
        _mediator = mediator;
    }

    [HttpGet]
    public async Task<ActionResult<IEnumerable<Product>>> GetProducts([FromQuery] string? category)
    {
        var products = await _mediator.Send(new Features.Products.Queries.GetAllProductsQuery(category));
        return Ok(products);
    }

    [HttpGet("{id}")]
    public async Task<ActionResult<Product>> GetProduct(int id)
    {
        var product = await _mediator.Send(new Features.Products.Queries.GetProductByIdQuery(id));
        if (product == null) return NotFound();
        return Ok(product);
    }

    [HttpPost]
    public async Task<ActionResult<Product>> CreateProduct(Product product)
    {
        var created = await _mediator.Send(new Features.Products.Commands.CreateProductCommand(product));
        return CreatedAtAction(nameof(GetProduct), new { id = created.Id }, created);
    }

    [HttpPut("{id}")]
    public async Task<IActionResult> UpdateProduct(int id, Product product)
    {
        if (id != product.Id) return BadRequest();
        var success = await _mediator.Send(new Features.Products.Commands.UpdateProductCommand(id, product));
        if (!success) return NotFound();
        return NoContent();
    }

    [HttpDelete("{id}")]
    public async Task<IActionResult> DeleteProduct(int id)
    {
        var success = await _mediator.Send(new Features.Products.Commands.DeleteProductCommand(id));
        if (!success) return NotFound();
        return NoContent();
    }
}
