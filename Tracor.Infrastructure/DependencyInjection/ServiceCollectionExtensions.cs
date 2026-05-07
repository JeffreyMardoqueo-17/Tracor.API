using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Tradecorp.Application.Abstractions.Persistence;
using Tradecorp.Infrastructure.Data;
using Tradecorp.Infrastructure.Persistence;
using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.Abstractions.Security;
using Tradecorp.Infrastructure.Services;

namespace Tradecorp.Infrastructure.DependencyInjection;

public static class ServiceCollectionExtensions
{
    public static IServiceCollection AddInfrastructure(this IServiceCollection services, IConfiguration configuration)
    {
        var connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontro la cadena de conexion 'DefaultConnection'.");

        services.AddDbContext<AppDbContext>(options =>
            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.MigrationsAssembly(typeof(AppDbContext).Assembly.GetName().Name);
                npgsql.EnableRetryOnFailure(
                    maxRetryCount: 3,
                    maxRetryDelay: TimeSpan.FromSeconds(10),
                    errorCodesToAdd: null);
            }));

        services.AddScoped<IDbConnectionFactory>(sp =>
        {
            var config = sp.GetRequiredService<IConfiguration>();
            var cs = config.GetConnectionString("DefaultConnection")
                ?? throw new InvalidOperationException("No se encontro la cadena de conexion 'DefaultConnection'.");
            return new NpgsqlDbConnectionFactory(cs);
        });
        
        // Users, auth and security
        services.AddScoped<IUserRepository, UserRepository>();
        services.AddScoped<IUserService, UserService>();
        services.AddScoped<IJwtService, JwtService>();
        services.AddScoped<IConfiguracionSistemaRepository, ConfiguracionSistemaRepository>();
        services.AddScoped<IConfiguracionSistemaService, ConfiguracionSistemaService>();

        // Clientes y Beneficiarios
        services.AddScoped<IClienteRepository, ClienteRepository>();
        services.AddScoped<IClienteBeneficiarioRepository, ClienteBeneficiarioRepository>();
        services.AddScoped<IClienteService, ClienteService>();
        services.AddScoped<IBeneficiarioService, BeneficiarioService>();

        // Bancos
        services.AddScoped<IBancoRepository, BancoRepository>();
        services.AddScoped<IBancoService, BancoService>();

        // Contratos
        services.AddScoped<IContratoRepository, ContratoRepository>();
        services.AddScoped<IContratoService, ContratoService>();

        // Pagos
        services.AddScoped<IPagoRepository, PagoRepository>();
        services.AddScoped<IPagoService, PagoService>();

        return services;
    }
}