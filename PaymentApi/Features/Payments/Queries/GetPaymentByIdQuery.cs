using MediatR;
using PaymentApi.DTOs;

namespace PaymentApi.Features.Payments.Queries;

public record GetPaymentByIdQuery(Guid Id) : IRequest<PaymentResponseDto?>;
