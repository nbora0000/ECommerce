using System.Threading;
using System.Threading.Tasks;
using CatalogApi.Features.Products.Handlers;
using CatalogApi.Features.Products.Queries;
using CatalogApi.Repositories;
using CatalogApi.Models;
using System.Collections.Generic;
using Moq;
using Xunit;

namespace CatalogApi.Tests;

public class ProductsHandlerTests
{
    [Fact]
    public async Task GetAllProductsHandler_ReturnsProducts()
    {
        var repoMock = new Mock<IProductRepository>();
        repoMock.Setup(r => r.GetAllProductsAsync(null)).ReturnsAsync(new List<Product> { new Product { Id = 1, Name = "P" } });

        var handler = new GetAllProductsHandler(repoMock.Object);
        var result = await handler.Handle(new GetAllProductsQuery(null), CancellationToken.None);

        Assert.Single(result);
    }
}
