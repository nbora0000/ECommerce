using MediatR;

namespace NotificationApi.Features.Notifications.Commands;

public record SendNotificationCommand(string Recipient, string Message, string Channel = "email") : IRequest<Unit>;
