using CatalogApi.Models;
using MediatR;

namespace CatalogApi.Features.Products.Queries;

public record GetAllProductsQuery(string? Category) : IRequest<IEnumerable<Product>>;
