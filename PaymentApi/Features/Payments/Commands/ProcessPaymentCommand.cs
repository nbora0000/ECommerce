using MediatR;
using PaymentApi.DTOs;

namespace PaymentApi.Features.Payments.Commands;

public record ProcessPaymentCommand(ProcessPaymentDto Dto) : IRequest<(PaymentResponseDto, bool)>;
