using MediatR;
using OrderApi.DTOs;
using SharedLibrary.Enums;

namespace OrderApi.Features.Orders.Queries;

public record GetAllOrdersQuery(OrderStatus? Status, int Page, int PageSize) : IRequest<(IEnumerable<OrderResponseDto>, int)>;
