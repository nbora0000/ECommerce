using CatalogApi.Features.Products.Commands;
using CatalogApi.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace CatalogApi.Features.Products.Handlers;

public class CreateProductHandler : IRequestHandler<CreateProductCommand, CatalogApi.Models.Product>
{
    private readonly CatalogDbContext _db;
    public CreateProductHandler(CatalogDbContext db) { _db = db; }
    public async Task<CatalogApi.Models.Product> Handle(CreateProductCommand request, CancellationToken cancellationToken)
    {
        await _db.Products.AddAsync(request.Product, cancellationToken);
        await _db.SaveChangesAsync(cancellationToken);
        return request.Product;
    }
}
