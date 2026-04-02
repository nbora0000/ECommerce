using CatalogApi.Features.Products.Queries;
using CatalogApi.Data;
using MediatR;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;

namespace CatalogApi.Features.Products.Handlers;

public class GetAllProductsHandler : IRequestHandler<GetAllProductsQuery, IEnumerable<CatalogApi.Models.Product>>
{
    private readonly CatalogDbContext _db;
    public GetAllProductsHandler(CatalogDbContext db) { _db = db; }

    public async Task<IEnumerable<CatalogApi.Models.Product>> Handle(GetAllProductsQuery request, CancellationToken cancellationToken)
    {
        var param = new SqlParameter("@Category", (object?)request.Category ?? DBNull.Value);
        var results = await _db.Products
            .FromSqlRaw("EXEC [dbo].[sp_GetAllProducts] @Category", param)
            .AsNoTracking()
            .ToListAsync(cancellationToken);
        return results;
    }
}
