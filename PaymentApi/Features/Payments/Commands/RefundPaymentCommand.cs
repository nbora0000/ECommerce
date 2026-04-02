using MediatR;
using PaymentApi.DTOs;

namespace PaymentApi.Features.Payments.Commands;

public record RefundPaymentCommand(Guid Id, RefundPaymentDto Dto) : IRequest<PaymentResponseDto?>;
