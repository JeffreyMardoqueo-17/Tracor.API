using System.Data;

namespace Tradecorp.Application.Abstractions.Persistence;

public interface IDbConnectionFactory
{
    IDbConnection CreateConnection();
}