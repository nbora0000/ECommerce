using Microsoft.EntityFrameworkCore;
using InventoryApi.Data;
using MediatR;
using System.Reflection;

var builder = WebApplication.CreateBuilder(args);

// ?? Services ??????????????????????????????????????????????????????????????????
builder.Services.AddControllers();
builder.Services.AddMediatR(typeof(Program).Assembly);

builder.Services.AddDbContext<InventoryDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("InventoryDb"),
        sqlOptions => sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    ));

// repository and service removed: handlers use DbContext directly

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "Inventory API",
        Version = "v1",
        Description = "Microservice for managing inventory and stock"
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);
});

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<InventoryDbContext>("database");

// ?? Middleware ?????????????????????????????????????????????????????????????????
var app = builder.Build();

// Auto-apply migrations on startup
using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<InventoryDbContext>();
    db.Database.Migrate();
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "Inventory API v1");
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
