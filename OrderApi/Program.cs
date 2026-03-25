using Microsoft.EntityFrameworkCore;
using OrderApi.Data;
using OrderApi.Repositories;
using OrderApi.Services;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddControllers();

builder.Services.AddDbContext<OrderDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("OrdersDb"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    ));

// ── Repository & Service Layer ────────────────────────────────────────────────
builder.Services.AddScoped<IOrderRepository, OrderRepository>();
builder.Services.AddScoped<IOrderService, OrderService>();

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Order API",
        Version = "v1",
        Description = "Microservice for managing customer orders"
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<OrderDbContext>("database");

// ── Middleware ─────────────────────────────────────────────────────────────────
var app = builder.Build();

// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<OrderDbContext>();
    db.Database.Migrate();

    // ── Create/update stored procedures (idempotent) ──────────────────────
    SharedLibrary.Data.StoredProcedureInitializer.ExecuteSql(db,
        @"CREATE OR ALTER PROCEDURE [dbo].[sp_GetAllOrders]
            @Status   NVARCHAR(50) = NULL,
            @Page     INT          = 1,
            @PageSize INT          = 20
          AS BEGIN SET NOCOUNT ON;
            SELECT Id, CustomerName, CustomerEmail, TotalAmount, Currency, Status, Notes, CreatedAt, UpdatedAt
            FROM [dbo].[Orders]
            WHERE (@Status IS NULL OR Status = @Status)
            ORDER BY CreatedAt DESC
            OFFSET ((@Page - 1) * @PageSize) ROWS FETCH NEXT @PageSize ROWS ONLY;
          END;",

        @"CREATE OR ALTER PROCEDURE [dbo].[sp_GetOrderById]
            @OrderId UNIQUEIDENTIFIER
          AS BEGIN SET NOCOUNT ON;
            SELECT Id, CustomerName, CustomerEmail, TotalAmount, Currency, Status, Notes, CreatedAt, UpdatedAt
            FROM [dbo].[Orders] WHERE Id = @OrderId;
            SELECT Id, OrderId, ProductId, ProductName, Quantity, UnitPrice
            FROM [dbo].[OrderItems] WHERE OrderId = @OrderId;
          END;",

        @"CREATE OR ALTER PROCEDURE [dbo].[sp_GetOrdersByStatus]
            @Status NVARCHAR(50)
          AS BEGIN SET NOCOUNT ON;
            SELECT Id, CustomerName, CustomerEmail, TotalAmount, Currency, Status, Notes, CreatedAt, UpdatedAt
            FROM [dbo].[Orders] WHERE Status = @Status ORDER BY CreatedAt DESC;
          END;",

        @"CREATE OR ALTER PROCEDURE [dbo].[sp_GetOrderCount]
            @Status NVARCHAR(50) = NULL
          AS BEGIN SET NOCOUNT ON;
            SELECT COUNT(*) AS TotalCount FROM [dbo].[Orders]
            WHERE (@Status IS NULL OR Status = @Status);
          END;"
    );
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Order API v1");
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
