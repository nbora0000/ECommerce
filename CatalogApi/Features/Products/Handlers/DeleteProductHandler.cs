using CatalogApi.Features.Products.Commands;
using CatalogApi.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CatalogApi.Features.Products.Handlers;

public class DeleteProductHandler : IRequestHandler<DeleteProductCommand, bool>
{
    private readonly CatalogDbContext _db;
    public DeleteProductHandler(CatalogDbContext db) { _db = db; }
    public async Task<bool> Handle(DeleteProductCommand request, CancellationToken cancellationToken)
    {
        var product = await _db.Products.FindAsync(new object[] { request.Id }, cancellationToken);
        if (product is null) return false;
        _db.Products.Remove(product);
        await _db.SaveChangesAsync(cancellationToken);
        return true;
    }
}
