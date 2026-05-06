namespace Tradecorp.Application.DTOs;

public record FechaCorteDto(int Id, int Orden, int Dia, int Mes);

public record CortePagoDto(
    int Id,
    string Nombre,
    bool Activo,
    IReadOnlyCollection<FechaCorteDto> Fechas);

public record SistemaConfiguracionDto(
    int Id,
    int DiasCuatrimestreBase,
    int DiasMesBase,
    decimal ComisionEmpresaPorcentaje,
    bool UsarDiasExactosPrimerCuatrimestre,
    bool AplicarReglaAnioBisiesto,
    bool Activo,
    DateTime FechaActualizacion,
    IReadOnlyCollection<CortePagoDto> CortesPago);

public record UpdateSistemaConfiguracionRequestDto(
    int DiasCuatrimestreBase,
    int DiasMesBase,
    decimal ComisionEmpresaPorcentaje,
    bool UsarDiasExactosPrimerCuatrimestre,
    bool AplicarReglaAnioBisiesto,
    bool Activo);

public record UpsertFechaCorteDto(int Orden, int Dia, int Mes);

public record UpsertCortePagoRequestDto(
    string Nombre,
    bool Activo,
    IReadOnlyCollection<UpsertFechaCorteDto> Fechas);
