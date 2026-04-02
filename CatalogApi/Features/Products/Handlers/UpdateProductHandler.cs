using CatalogApi.Features.Products.Commands;
using CatalogApi.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CatalogApi.Features.Products.Handlers;

public class UpdateProductHandler : IRequestHandler<UpdateProductCommand, bool>
{
    private readonly CatalogDbContext _db;
    public UpdateProductHandler(CatalogDbContext db) { _db = db; }
    public async Task<bool> Handle(UpdateProductCommand request, CancellationToken cancellationToken)
    {
        if (request.Id != request.Product.Id) return false;
        try
        {
            _db.Products.Update(request.Product);
            await _db.SaveChangesAsync(cancellationToken);
            return true;
        }
        catch (DbUpdateConcurrencyException)
        {
            var exists = await _db.Products.AnyAsync(p => p.Id == request.Id, cancellationToken);
            if (!exists) return false;
            throw;
        }
    }
}
