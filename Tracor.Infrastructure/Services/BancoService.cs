using Tradecorp.Application.Abstractions.Persistence;
using Tradecorp.Application.Abstractions.Services;
using Tradecorp.Application.DTOs;
using Tradecorp.Domain.Models.Entities;

namespace Tradecorp.Infrastructure.Services
{
    public class BancoService : IBancoService
    {
        private readonly IBancoRepository _bancoRepository;

        public BancoService(IBancoRepository bancoRepository)
        {
            _bancoRepository = bancoRepository;
        }

        public async Task<BancoResponse?> GetByIdAsync(int id)
        {
            var banco = await _bancoRepository.GetByIdAsync(id);
            if (banco == null) return null;

            return new BancoResponse
            {
                Id = banco.Id,
                Nombre = banco.Nombre,
            };
        }

        public async Task<IEnumerable<BancoResponse>> GetAllAsync()
        {
            var bancos = await _bancoRepository.GetAllAsync();
            return bancos.Select(b => new BancoResponse
            {
                Id = b.Id,
                Nombre = b.Nombre,
            });
        }

        public async Task<BancoResponse> CreateAsync(CreateBancoRequest request)
        {
            if (request == null)
                throw new ArgumentNullException(nameof(request));

            if (string.IsNullOrWhiteSpace(request.Nombre))
                throw new ArgumentException("El nombre del banco es obligatorio.");

            var nombreNormalizado = request.Nombre.Trim();

            var existeBanco = await _bancoRepository.ExistsByNameAsync(nombreNormalizado);
            if (existeBanco)
                throw new InvalidOperationException("Ya existe un banco con ese nombre.");

            var banco = new Banco
            {
                Nombre = nombreNormalizado,
            };

            await _bancoRepository.CreateAsync(banco);

            return new BancoResponse
            {
                Id = banco.Id,
                Nombre = banco.Nombre,
            };
        }
    }
}