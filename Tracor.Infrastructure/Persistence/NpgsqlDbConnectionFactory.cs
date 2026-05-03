using System.Data;
using Microsoft.Extensions.Configuration;
using Npgsql;
using Tradecorp.Application.Abstractions.Persistence;

namespace Tradecorp.Infrastructure.Persistence;

public sealed class NpgsqlDbConnectionFactory : IDbConnectionFactory
{
    private readonly string connectionString;

    public NpgsqlDbConnectionFactory(IConfiguration configuration)
    {
        connectionString = configuration.GetConnectionString("DefaultConnection")
            ?? throw new InvalidOperationException("No se encontro la cadena de conexion 'DefaultConnection'.");
    }

    public IDbConnection CreateConnection()
    {
        return new NpgsqlConnection(connectionString);
    }
}