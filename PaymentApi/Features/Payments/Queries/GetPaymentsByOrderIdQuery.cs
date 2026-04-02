using MediatR;
using PaymentApi.DTOs;

namespace PaymentApi.Features.Payments.Queries;

public record GetPaymentsByOrderIdQuery(Guid OrderId) : IRequest<IEnumerable<PaymentResponseDto>>;
