using MediatR;
using NotificationApi.Features.Notifications.Commands;
using Microsoft.Extensions.Logging;

namespace NotificationApi.Features.Notifications.Handlers;

public class SendNotificationHandler : IRequestHandler<SendNotificationCommand, Unit>
{
    private readonly ILogger<SendNotificationHandler> _logger;
    public SendNotificationHandler(ILogger<SendNotificationHandler> logger) { _logger = logger; }
    public Task<Unit> Handle(SendNotificationCommand request, CancellationToken cancellationToken)
    {
        _logger.LogInformation("Sending notification to {Recipient} via {Channel}: {Message}", request.Recipient, request.Channel, request.Message);
        return Task.FromResult(Unit.Value);
    }
}
