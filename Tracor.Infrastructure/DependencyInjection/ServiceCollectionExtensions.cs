using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tradecorp.Application.Abstractions.Persistence;
using Tradecorp.Infrastructure.Data;
using Tradecorp.Infrastructure.Persistence;

namespace Tradecorp.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontro la cadena de conexion 'DefaultConnection'.");

        services.AddDbContext<AppDbContext>(options => options.UseNpgsql(connectionString));
        services.AddScoped<IDbConnectionFactory, NpgsqlDbConnectionFactory>();

        return services;
    }
}