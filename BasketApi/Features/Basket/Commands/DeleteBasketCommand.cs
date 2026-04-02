using MediatR;

namespace BasketApi.Features.Basket.Commands;

public record DeleteBasketCommand(string CustomerId) : IRequest<bool>;
