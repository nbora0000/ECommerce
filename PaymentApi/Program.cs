using Microsoft.EntityFrameworkCore;
using PaymentApi.Data;
using MediatR;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddMediatR(typeof(Program).Assembly);

builder.Services.AddDbContext<PaymentDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("PaymentsDb"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    ));


builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Payment API",
        Version = "v1",
        Description = "Microservice for processing and managing payments"
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<PaymentDbContext>("database");

// ── Middleware ─────────────────────────────────────────────────────────────────
var app = builder.Build();

// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<PaymentDbContext>();
    db.Database.Migrate();

    // ── Create/update stored procedures (idempotent) ──────────────────────
    SharedLibrary.Data.StoredProcedureInitializer.ExecuteSql(db,
        @"CREATE OR ALTER PROCEDURE [dbo].[sp_GetAllPayments]
            @Status   NVARCHAR(50) = NULL,
            @Page     INT          = 1,
            @PageSize INT          = 20
          AS BEGIN SET NOCOUNT ON;
            SELECT [Id], [OrderId], [Amount], [Currency], [Status], [Method], [TransactionId],
                   [FailureReason], [RefundTransactionId], [RefundedAt], [CreatedAt], [UpdatedAt]
            FROM [dbo].[Payments]
            WHERE (@Status IS NULL OR [Status] = @Status)
            ORDER BY [CreatedAt] DESC
            OFFSET ((@Page - 1) * @PageSize) ROWS FETCH NEXT @PageSize ROWS ONLY;
          END;",

        @"CREATE OR ALTER PROCEDURE [dbo].[sp_GetPaymentById]
            @PaymentId UNIQUEIDENTIFIER
          AS BEGIN SET NOCOUNT ON;
            SELECT [Id], [OrderId], [Amount], [Currency], [Status], [Method], [TransactionId],
                   [FailureReason], [RefundTransactionId], [RefundedAt], [CreatedAt], [UpdatedAt]
            FROM [dbo].[Payments] WHERE [Id] = @PaymentId;
          END;",

        @"CREATE OR ALTER PROCEDURE [dbo].[sp_GetPaymentsByOrderId]
            @OrderId UNIQUEIDENTIFIER
          AS BEGIN SET NOCOUNT ON;
            SELECT [Id], [OrderId], [Amount], [Currency], [Status], [Method], [TransactionId],
                   [FailureReason], [RefundTransactionId], [RefundedAt], [CreatedAt], [UpdatedAt]
            FROM [dbo].[Payments] WHERE [OrderId] = @OrderId ORDER BY [CreatedAt] DESC;
          END;",

        @"CREATE OR ALTER PROCEDURE [dbo].[sp_GetPaymentsByStatus]
            @Status NVARCHAR(50)
          AS BEGIN SET NOCOUNT ON;
            SELECT [Id], [OrderId], [Amount], [Currency], [Status], [Method], [TransactionId],
                   [FailureReason], [RefundTransactionId], [RefundedAt], [CreatedAt], [UpdatedAt]
            FROM [dbo].[Payments] WHERE [Status] = @Status ORDER BY [CreatedAt] DESC;
          END;",

        @"CREATE OR ALTER PROCEDURE [dbo].[sp_GetPaymentCount]
            @Status NVARCHAR(50) = NULL
          AS BEGIN SET NOCOUNT ON;
            SELECT COUNT(*) AS [TotalCount] FROM [dbo].[Payments]
            WHERE (@Status IS NULL OR [Status] = @Status);
          END;",

        @"CREATE OR ALTER PROCEDURE [dbo].[sp_CheckCompletedPaymentExists]
            @OrderId UNIQUEIDENTIFIER
          AS BEGIN SET NOCOUNT ON;
            SELECT CASE WHEN EXISTS (
                SELECT 1 FROM [dbo].[Payments] WHERE [OrderId] = @OrderId AND [Status] = 'Completed'
            ) THEN 1 ELSE 0 END AS [Exists];
          END;"
    );
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Payment API v1");
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
