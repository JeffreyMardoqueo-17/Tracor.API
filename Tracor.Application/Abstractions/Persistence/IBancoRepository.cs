using Tradecorp.Domain.Models.Entities;

namespace Tradecorp.Application.Abstractions.Persistence
{
    public interface IBancoRepository
    {
        Task CreateAsync(Banco banco);
        Task<IEnumerable<Banco>> GetAllAsync();
        Task<Banco?> GetByIdAsync(int id);
        Task<bool> ExistsByNameAsync(string nombre);
    }
}