using MediatR;

namespace InventoryApi.Features.Inventory.Commands;

public record ReserveInventoryCommand(Guid ProductId, Guid OrderId, int Quantity) : IRequest<bool>;
