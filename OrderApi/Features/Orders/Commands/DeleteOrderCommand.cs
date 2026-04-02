using MediatR;

namespace OrderApi.Features.Orders.Commands;

public record DeleteOrderCommand(Guid Id) : IRequest<bool>;
