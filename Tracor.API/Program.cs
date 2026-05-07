using Tradecorp.Infrastructure.DependencyInjection;
using Tradecorp.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;
using Microsoft.IdentityModel.Tokens;
using System.Text;
using Tradecorp.Infrastructure.Data;
using Tradecorp.Domain.Models.Entities;
using Tradecorp.Domain.Models.Enums;
using BCrypt.Net;
using System.Threading.RateLimiting;
using Microsoft.AspNetCore.RateLimiting; //esto es para evitar la sobre carga en el servidor

var builder = WebApplication.CreateBuilder(args);

var frontendOrigins = builder.Configuration
    .GetSection("Cors:AllowedOrigins")
    .Get<string[]>()
    ?? [
        "http://localhost:3000",
        "https://localhost:3000",
        "http://127.0.0.1:3000",
        "https://127.0.0.1:3000"
    ];

// Add services to the container.

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        //esto permite que los enums se serialicen como strings en lugar de numeros
        options.JsonSerializerOptions.Converters.Add(new System.Text.Json.Serialization.JsonStringEnumConverter());
        //esto evita errores de referencia circular
        options.JsonSerializerOptions.ReferenceHandler = System.Text.Json.Serialization.ReferenceHandler.IgnoreCycles;
    });
builder.Services.AddCors(options =>
{
    options.AddPolicy("Frontend", policy =>
    {
        policy
            .WithOrigins(frontendOrigins)
            .AllowAnyHeader()
            .AllowAnyMethod()
            .AllowCredentials();
    });
});
// Learn more about configuring Swagger/OpenAPI at https://aka.ms/aspnetcore/swashbuckle
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen();
builder.Services.AddInfrastructure(builder.Configuration);

// JWT Authentication
var jwtKey = builder.Configuration["Jwt:Key"] ?? throw new InvalidOperationException("Jwt:Key not configured");
var issuer = builder.Configuration["Jwt:Issuer"] ?? "Tradecorp";
var audience = builder.Configuration["Jwt:Audience"] ?? "TradecorpClients";

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode = StatusCodes.Status429TooManyRequests; // Código 429

    options.AddFixedWindowLimiter(policyName: "clientesPolicy", limiterOptions =>
    {
        limiterOptions.PermitLimit = 10; // Máximo 10 solicitudes
        limiterOptions.Window = TimeSpan.FromSeconds(10); // En 10 segundos
        limiterOptions.QueueProcessingOrder = QueueProcessingOrder.OldestFirst;
        limiterOptions.QueueLimit = 0; // No encolar, rechazar inmediatamente
    });
});

builder.Services.AddAuthentication(options =>
{
    options.DefaultAuthenticateScheme = JwtBearerDefaults.AuthenticationScheme;
    options.DefaultChallengeScheme = JwtBearerDefaults.AuthenticationScheme;
})
    .AddJwtBearer(options =>
    {
        options.Events = new JwtBearerEvents
        {
            OnMessageReceived = context =>
            {
                if (context.Request.Cookies.TryGetValue("access_token", out var token))
                {
                    context.Token = token;
                }

                return Task.CompletedTask;
            }
        };

        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuerSigningKey = true,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey)),
            ValidateIssuer = true,
            ValidIssuer = issuer,
            ValidateAudience = true,
            ValidAudience = audience,
            NameClaimType = ClaimTypes.Name,
            RoleClaimType = ClaimTypes.Role,
            ValidateLifetime = true,
            ClockSkew = TimeSpan.Zero
        };
    });

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
}

if (app.Environment.IsDevelopment())
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();

    // Seed Usuario admin if not exists
    if (!await db.Usuarios.AnyAsync())
    {
        db.Usuarios.Add(new Usuario
        {
            Nombre = "Administrador",
            Email = "admin@tradecorp.com",
            PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin123!"),
            Rol = UsuarioRol.Admin,
            Activo = true,
            FechaCreacion = DateTime.UtcNow
        });

        await db.SaveChangesAsync();
        app.Logger.LogWarning("Seed de desarrollo creado: admin@tradecorp.com / Admin123!");
    }
}

// Configure the HTTP request pipeline.
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}
app.UseRateLimiter();
app.UseGlobalExceptionHandling();

app.UseCors("Frontend");
app.UseHttpsRedirection();

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

app.Run();
