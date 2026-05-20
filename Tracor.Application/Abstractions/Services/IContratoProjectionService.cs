using Tradecorp.Application.DTOs;

namespace Tradecorp.Application.Abstractions.Services;

public interface IContratoProjectionService
{
    Task<ContratoProjectionResponse?> ObtenerProyeccion24MesesAsync(int contratoId);
    Task<ContratoProjectionResponse> SimularAsync(SimularContratoRequest request);
}
