using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Tradecorp.Application.DTOs;

namespace Tradecorp.Application.Abstractions.Services
{
    public interface IBancoService
    {
        Task<BancoResponse?> GetByIdAsync(int id);
        Task<IEnumerable<BancoResponse>> GetAllAsync();
        Task<BancoResponse> CreateAsync(CreateBancoRequest request);
    }
}