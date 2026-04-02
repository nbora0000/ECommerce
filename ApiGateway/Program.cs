using Ocelot.DependencyInjection;
using Ocelot.Middleware;

var builder = WebApplication.CreateBuilder(args);

//  Add Health Checks to the Gateway itself
builder.Services.AddHealthChecks();

//  Add Ocelot Configuration
builder.Configuration.AddJsonFile("ocelot.json", optional: false, reloadOnChange: true);

//  Add Ocelot Services
builder.Services.AddOcelot(builder.Configuration);

//  Add Logging for easier troubleshooting
builder.Logging.AddConsole();

var app = builder.Build();

//  Map health check BEFORE Ocelot middleware so it doesn't get intercepted
app.MapHealthChecks("/health");

//  Optional: Simple root mapping before Ocelot
app.MapGet("/", () => "API Gateway is Running!");

//  Use Ocelot Middleware
await app.UseOcelot();

app.Run();
