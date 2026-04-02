using MediatR;
using OrderApi.DTOs;

namespace OrderApi.Features.Orders.Queries;

public record GetOrderByIdQuery(Guid Id) : IRequest<OrderResponseDto?>;
