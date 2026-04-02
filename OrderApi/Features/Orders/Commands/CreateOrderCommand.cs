using MediatR;
using OrderApi.DTOs;

namespace OrderApi.Features.Orders.Commands;

public record CreateOrderCommand(CreateOrderDto Dto) : IRequest<OrderResponseDto>;
