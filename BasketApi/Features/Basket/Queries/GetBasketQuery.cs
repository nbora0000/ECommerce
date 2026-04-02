using BasketApi.Models;
using MediatR;

namespace BasketApi.Features.Basket.Queries;

public record GetBasketQuery(string CustomerId) : IRequest<ShoppingCart?>;
