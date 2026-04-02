using MediatR;
using PaymentApi.DTOs;
using SharedLibrary.Enums;

namespace PaymentApi.Features.Payments.Queries;

public record GetAllPaymentsQuery(PaymentStatus? Status, int Page, int PageSize) : IRequest<(IEnumerable<PaymentResponseDto>, int)>;
