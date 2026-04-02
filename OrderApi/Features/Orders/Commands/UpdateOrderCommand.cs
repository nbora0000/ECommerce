using MediatR;
using OrderApi.DTOs;

namespace OrderApi.Features.Orders.Commands;

public record UpdateOrderCommand(Guid Id, UpdateOrderDto Dto) : IRequest<OrderResponseDto?>;
