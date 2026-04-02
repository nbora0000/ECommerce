using MediatR;

namespace CatalogApi.Features.Products.Commands;

public record DeleteProductCommand(int Id) : IRequest<bool>;
