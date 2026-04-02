using CatalogApi.Models;
using MediatR;

namespace CatalogApi.Features.Products.Commands;

public record UpdateProductCommand(int Id, Product Product) : IRequest<bool>;
