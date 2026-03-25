using CatalogApi.Data;
using CatalogApi.Repositories;
using CatalogApi.Services;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddControllers();

builder.Services.AddDbContext<CatalogDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("CatalogDb"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    ));

// ── Repository & Service Layer ────────────────────────────────────────────────
builder.Services.AddScoped<IProductRepository, ProductRepository>();
builder.Services.AddScoped<IProductService, ProductService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Catalog API",
        Version = "v1",
        Description = "Microservice for managing product catalog"
    });
});

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<CatalogDbContext>("database");

// ── Middleware ─────────────────────────────────────────────────────────────────
var app = builder.Build();

// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<CatalogDbContext>();
    db.Database.Migrate();

    // ── Create/update stored procedures (idempotent) ──────────────────────
    SharedLibrary.Data.StoredProcedureInitializer.ExecuteSql(db,
        @"CREATE OR ALTER PROCEDURE [dbo].[sp_GetAllProducts]
            @Category NVARCHAR(200) = NULL
          AS BEGIN SET NOCOUNT ON;
            SELECT Id, Name, Description, Price, Category, ImageUrl, StockQuantity
            FROM [dbo].[Products]
            WHERE (@Category IS NULL OR Category = @Category)
            ORDER BY Name;
          END;",

        @"CREATE OR ALTER PROCEDURE [dbo].[sp_GetProductById]
            @ProductId INT
          AS BEGIN SET NOCOUNT ON;
            SELECT Id, Name, Description, Price, Category, ImageUrl, StockQuantity
            FROM [dbo].[Products] WHERE Id = @ProductId;
          END;",

        @"CREATE OR ALTER PROCEDURE [dbo].[sp_GetProductsByCategory]
            @Category NVARCHAR(200)
          AS BEGIN SET NOCOUNT ON;
            SELECT Id, Name, Description, Price, Category, ImageUrl, StockQuantity
            FROM [dbo].[Products] WHERE Category = @Category ORDER BY Name;
          END;",

        @"CREATE OR ALTER PROCEDURE [dbo].[sp_SearchProducts]
            @SearchTerm NVARCHAR(200)
          AS BEGIN SET NOCOUNT ON;
            SELECT Id, Name, Description, Price, Category, ImageUrl, StockQuantity
            FROM [dbo].[Products]
            WHERE Name LIKE '%' + @SearchTerm + '%' OR Description LIKE '%' + @SearchTerm + '%'
            ORDER BY Name;
          END;"
    );
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Catalog API v1");
    c.RoutePrefix = "swagger";
});
// Redirect root URL to swagger
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});
app.UseCors();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
