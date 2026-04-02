using CatalogApi.Models;
using MediatR;

namespace CatalogApi.Features.Products.Commands;

public record CreateProductCommand(Product Product) : IRequest<Product>;
