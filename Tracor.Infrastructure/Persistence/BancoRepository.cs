using Microsoft.EntityFrameworkCore;
using Tradecorp.Application.Abstractions.Persistence;
using Tradecorp.Domain.Models.Entities;
using Tradecorp.Infrastructure.Data;

namespace Tradecorp.Infrastructure.Persistence
{
    public class BancoRepository : IBancoRepository
    {
        private readonly AppDbContext _context;

        public BancoRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task CreateAsync(Banco banco)
        {
            await _context.Bancos.AddAsync(banco);
            await _context.SaveChangesAsync();
        }

        public async Task<IEnumerable<Banco>> GetAllAsync()
        {
            return await _context.Bancos
                .AsNoTracking()
                .OrderBy(b => b.Nombre)
                .ToListAsync();
        }

        public async Task<Banco?> GetByIdAsync(int id)
        {
            return await _context.Bancos
                .AsNoTracking()
                .FirstOrDefaultAsync(b => b.Id == id);
        }

        public async Task<bool> ExistsByNameAsync(string nombre)
        {
            return await _context.Bancos
                .AnyAsync(b => b.Nombre == nombre);
        }
    }
}