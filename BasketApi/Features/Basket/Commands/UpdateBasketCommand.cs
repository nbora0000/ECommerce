using BasketApi.Models;
using MediatR;

namespace BasketApi.Features.Basket.Commands;

public record UpdateBasketCommand(ShoppingCart Basket) : IRequest<ShoppingCart>;
