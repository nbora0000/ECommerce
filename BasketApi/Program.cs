using BasketApi.Data;
using MediatR;
using Microsoft.EntityFrameworkCore;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ── Services ──────────────────────────────────────────────────────────────────
builder.Services.AddControllers();
builder.Services.AddMediatR(typeof(Program).Assembly);

builder.Services.AddDbContext<BasketDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("BasketDb"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    ));

// ── Repository & Service Layer ────────────────────────────────────────────────
// repository and service removed: handlers use DbContext directly

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Basket API",
        Version = "v1",
        Description = "Microservice for managing customer shopping carts"
    });
});

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<BasketDbContext>("database");

// ── Middleware ─────────────────────────────────────────────────────────────────
var app = builder.Build();

// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<BasketDbContext>();
    db.Database.Migrate();

    // ── Create/update stored procedures (idempotent) ──────────────────────
    SharedLibrary.Data.StoredProcedureInitializer.ExecuteSql(db,
        @"CREATE OR ALTER PROCEDURE [dbo].[sp_GetBasketByCustomerId]
            @CustomerId NVARCHAR(450)
          AS BEGIN SET NOCOUNT ON;
            SELECT CustomerId, CreatedAt, UpdatedAt FROM [dbo].[ShoppingCarts] WHERE CustomerId = @CustomerId;
          END;"
    );
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Basket API v1");
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
