using CatalogApi.Features.Products.Queries;
using CatalogApi.Data;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CatalogApi.Features.Products.Handlers;

public class GetProductByIdHandler : IRequestHandler<GetProductByIdQuery, CatalogApi.Models.Product?>
{
    private readonly CatalogDbContext _db;
    public GetProductByIdHandler(CatalogDbContext db) { _db = db; }
    public async Task<CatalogApi.Models.Product?> Handle(GetProductByIdQuery request, CancellationToken cancellationToken)
    {
        var param = new SqlParameter("@ProductId", request.Id);
        var results = await _db.Products.FromSqlRaw("EXEC [dbo].[sp_GetProductById] @ProductId", param).ToListAsync(cancellationToken);
        return results.FirstOrDefault();
    }
}
