using CatalogApi.Models;
using MediatR;

namespace CatalogApi.Features.Products.Queries;

public record GetProductByIdQuery(int Id) : IRequest<Product?>;
