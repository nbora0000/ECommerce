using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using System.Text;
using UserApi.Data;
using System.Reflection;
using MediatR;

var builder = WebApplication.CreateBuilder(args);

// ?? Services ??????????????????????????????????????????????????????????????????
builder.Services.AddControllers();
builder.Services.AddMediatR(typeof(Program).Assembly);

// Resolve connection string with a fallback and fail fast if it's not configured
var usersConnection = builder.Configuration.GetConnectionString("UsersDb")
                      ?? builder.Configuration.GetConnectionString("DefaultConnection");

if (string.IsNullOrWhiteSpace(usersConnection))
    throw new InvalidOperationException("Connection string 'UsersDb' (or 'DefaultConnection') is not configured. Set 'ConnectionStrings:UsersDb' in appsettings or environment variables.");

builder.Services.AddDbContext<UserDbContext>(options =>
    options.UseSqlServer(
        usersConnection,
        sqlOptions => sqlOptions.EnableRetryOnFailure(5, TimeSpan.FromSeconds(10), null)
    ));



var key = builder.Configuration["Jwt:Key"] ?? "SUPER_SECRET_KEY_1234567890123456_EXTENDED_KEY_48";
builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
}).AddJwtBearer(options =>
{
    options.RequireHttpsMetadata = false;
    options.SaveToken = true;
    options.TokenValidationParameters = new TokenValidationParameters
    {
        ValidateIssuerSigningKey = true,
        IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(key)),
        ValidateIssuer = false,
        ValidateAudience = false,
    };
});

builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new()
    {
        Title = "User API",
        Version = "v1",
        Description = "Microservice for managing users and authentication"
    });
    var xmlFile = $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";
    var xmlPath = Path.Combine(AppContext.BaseDirectory, xmlFile);
    if (File.Exists(xmlPath)) c.IncludeXmlComments(xmlPath);

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                },
                Scheme = "oauth2",
                Name = "Bearer",
                In = ParameterLocation.Header
            },
            new List<string>()
        }
    });
});

builder.Services.AddCors(options =>
    options.AddDefaultPolicy(policy =>
        policy.AllowAnyOrigin().AllowAnyHeader().AllowAnyMethod()));

builder.Services.AddHealthChecks()
    .AddDbContextCheck<UserDbContext>("database");

// ?? Middleware ?????????????????????????????????????????????????????????????????
var app = builder.Build();

// Auto-apply migrations on startup with retries to handle DB not yet ready or unreachable
using (var scope = app.Services.CreateScope())
{
    var logger = scope.ServiceProvider.GetRequiredService<ILogger<Program>>();
    var db = scope.ServiceProvider.GetRequiredService<UserDbContext>();
    const int maxAttempts = 10;
    var delay = TimeSpan.FromSeconds(5);
    var attempt = 0;

    while (attempt < maxAttempts)
    {
        attempt++;
        try
        {
            if (db.Database.CanConnect())
            {
                db.Database.Migrate();
                logger.LogInformation("Database migrations applied on attempt {Attempt}.", attempt);
                break;
            }

            logger.LogWarning("Database is not reachable (attempt {Attempt}/{Max}). Retrying in {Delay}s...", attempt, maxAttempts, delay.TotalSeconds);
        }
        catch (Exception ex)
        {
            logger.LogWarning(ex, "Exception while attempting to connect to database (attempt {Attempt}/{Max}). Retrying in {Delay}s...", attempt, maxAttempts, delay.TotalSeconds);
        }

        if (attempt == maxAttempts)
        {
            logger.LogError("Could not connect to the database after {Max} attempts. Application startup will stop.", maxAttempts);
            throw new InvalidOperationException($"Could not connect to the database after {maxAttempts} attempts. See logs for details.");
        }

        System.Threading.Thread.Sleep(delay);
    }
}

app.UseSwagger();
app.UseSwaggerUI(c =>
{
    c.SwaggerEndpoint("/swagger/v1/swagger.json", "User API v1");
    c.RoutePrefix = "swagger";
});
// Redirect root URL to swagger
app.MapGet("/", context =>
{
    context.Response.Redirect("/swagger");
    return Task.CompletedTask;
});

app.UseCors();
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();
app.MapHealthChecks("/health");

app.Run();
