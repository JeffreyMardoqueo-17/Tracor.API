using Dapper;
using System.Data;
using Tradecorp.Domain.Models.Entities;
using Tradecorp.Application.Abstractions.Persistence;
using Tradecorp.Application.DTOs;

namespace Tradecorp.Infrastructure.Persistence;

public class ContratoDapperRepository : IContratoRepository
{
    private readonly IDbConnectionFactory _connectionFactory;

    public ContratoDapperRepository(IDbConnectionFactory connectionFactory)
    {
        _connectionFactory = connectionFactory ?? throw new ArgumentNullException(nameof(connectionFactory));
    }

    public Task<Contrato> CreateAsync(Contrato contrato) => throw new NotImplementedException();
    public Task<Contrato?> GetByIdAsync(int id) => throw new NotImplementedException();
    public Task<Contrato?> GetByNumeroAsync(string numero) => throw new NotImplementedException();
    public Task<IEnumerable<Contrato>> GetActivosPorClienteAsync(int clienteId) => throw new NotImplementedException();
    public Task<IEnumerable<Contrato>> GetPorClienteAsync(int clienteId) => throw new NotImplementedException();
    public Task<Contrato> UpdateAsync(Contrato contrato) => throw new NotImplementedException();
    public Task<bool> DeleteAsync(int id) => throw new NotImplementedException();
    public Task<string> GetProximoNumeroContratoAsync() => throw new NotImplementedException();

    public async Task<bool> ClienteExisteAsync(int clienteId)
    {
        const string sql = @"SELECT EXISTS(SELECT 1 FROM ""Clientes"" WHERE ""Id"" = @ClienteId)";
        using var conn = _connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<bool>(sql, new { ClienteId = clienteId });
    }

    public async Task<bool> ContratoExisteAsync(int contratoId)
    {
        const string sql = @"SELECT EXISTS(SELECT 1 FROM ""Contratos"" WHERE ""Id"" = @ContratoId)";
        using var conn = _connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<bool>(sql, new { ContratoId = contratoId });
    }

    public async Task<bool> ContratoActivoAsync(int contratoId)
    {
        const string sql = @"
SELECT EXISTS(
    SELECT 1
    FROM ""Contratos""
    WHERE ""Id"" = @ContratoId
      AND ""Estado"" = 'Activo'
)";
        using var conn = _connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<bool>(sql, new { ContratoId = contratoId });
    }

    public async Task<int> ObtenerClienteIdContratoAsync(int contratoId)
    {
        const string sql = @"SELECT ""ClienteId"" FROM ""Contratos"" WHERE ""Id"" = @ContratoId";
        using var conn = _connectionFactory.CreateConnection();
        return await conn.ExecuteScalarAsync<int>(sql, new { ContratoId = contratoId });
    }

    public async Task<string> GenerarNumeroContratoAsync(int clienteId)
    {
        const string sql = @"
SELECT COALESCE(MAX(SUBSTRING(""NumeroContrato"" FROM '([0-9]+)$')::INT), 0)
FROM ""Contratos""
WHERE ""ClienteId"" = @ClienteId";

        using var conn = _connectionFactory.CreateConnection();
        var secuencia = await conn.ExecuteScalarAsync<int>(sql, new { ClienteId = clienteId });
        return $"CTR-{clienteId:D6}-{(secuencia + 1):D4}";
    }

    public async Task<int> CrearContratoAsync(CreateContratoRequest request, int usuarioId, string numeroContrato)
    {
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();
        try
        {
            const string insertContratoSql = @"
INSERT INTO ""Contratos"" (
    ""ClienteId"", ""NumeroContrato"", ""FechaInicio"", ""CapitalInicial"", ""CapitalActual"",
    ""PorcentajeMensual"", ""ComisionRetiro"", ""ModalidadRendimiento"", ""Estado"", ""PermiteUnificacion""
)
VALUES (
    @ClienteId, @NumeroContrato, @FechaInicio, @CapitalInicial, @CapitalActual,
    @PorcentajeMensual, @ComisionRetiro, @ModalidadRendimiento, 'Activo', @PermiteUnificacion
)
RETURNING ""Id"";";

            var contratoId = await conn.ExecuteScalarAsync<int>(insertContratoSql, new
            {
                request.ClienteId,
                NumeroContrato = numeroContrato,
                FechaInicio = request.FechaInicio ?? DateOnly.FromDateTime(DateTime.UtcNow),
                request.CapitalInicial,
                CapitalActual = request.CapitalInicial,
                request.PorcentajeMensual,
                ComisionRetiro = request.ComisionRetiro ?? 0m,
                request.ModalidadRendimiento,
                request.PermiteUnificacion
            }, tx);

            const string insertConfigSql = @"
INSERT INTO ""ConfiguracionContrato"" (
    ""ContratoId"", ""Tipo"", ""CapitalBase"", ""PorcentajeMensual"", ""FechaInicio""
)
VALUES (
    @ContratoId, 'Inicial', @CapitalBase, @PorcentajeMensual, @FechaInicio
);";

            await conn.ExecuteAsync(insertConfigSql, new
            {
                ContratoId = contratoId,
                CapitalBase = request.CapitalInicial,
                request.PorcentajeMensual,
                FechaInicio = request.FechaInicio ?? DateOnly.FromDateTime(DateTime.UtcNow)
            }, tx);

            await InsertAuditoriaAsync(conn, tx, contratoId, "Creacion", null,
                new
                {
                    request.CapitalInicial,
                    request.PorcentajeMensual,
                    request.ModalidadRendimiento,
                    request.PermiteUnificacion,
                    NumeroContrato = numeroContrato
                }, request.Observacion, usuarioId);

            if (request.Beneficiarios is { Count: > 0 })
            {
                await AsignarBeneficiariosInternalAsync(conn, tx, contratoId, request.Beneficiarios, usuarioId);
            }

            tx.Commit();
            return contratoId;
        }
        catch
        {
            tx.Rollback();
            throw;
        }
    }

    public async Task<bool> ActualizarContratoAsync(int contratoId, UpdateContratoRequest request, int usuarioId)
    {
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        const string currentSql = @"
SELECT ""PorcentajeMensual"", ""ComisionRetiro"", ""ModalidadRendimiento"", ""PermiteUnificacion""
FROM ""Contratos""
WHERE ""Id"" = @ContratoId AND ""Estado"" = 'Activo';";

        var current = await conn.QuerySingleOrDefaultAsync(currentSql, new { ContratoId = contratoId }, tx);
        if (current is null)
            return false;

        const string updateSql = @"
UPDATE ""Contratos""
SET
    ""PorcentajeMensual"" = COALESCE(@PorcentajeMensual, ""PorcentajeMensual""),
    ""ComisionRetiro"" = COALESCE(@ComisionRetiro, ""ComisionRetiro""),
    ""ModalidadRendimiento"" = COALESCE(@ModalidadRendimiento, ""ModalidadRendimiento""),
    ""PermiteUnificacion"" = COALESCE(@PermiteUnificacion, ""PermiteUnificacion""),
    ""FechaActualizacion"" = NOW()
WHERE ""Id"" = @ContratoId;";

        await conn.ExecuteAsync(updateSql, new
        {
            ContratoId = contratoId,
            request.PorcentajeMensual,
            request.ComisionRetiro,
            request.ModalidadRendimiento,
            request.PermiteUnificacion
        }, tx);

        await InsertAuditoriaAsync(conn, tx, contratoId, "Actualizacion", current,
            new
            {
                request.PorcentajeMensual,
                request.ComisionRetiro,
                request.ModalidadRendimiento,
                request.PermiteUnificacion
            }, request.Observacion, usuarioId);

        if (request.PorcentajeMensual.HasValue)
        {
            await InsertConfiguracionAsync(conn, tx, contratoId, "Ajuste", null, request.PorcentajeMensual.Value);
            await InsertAuditoriaAsync(conn, tx, contratoId, "CambioPorcentaje", null,
                new { request.PorcentajeMensual }, request.Observacion, usuarioId);
        }

        tx.Commit();
        return true;
    }

    public async Task<bool> FinalizarContratoAsync(int contratoId, int usuarioId, string? observacion)
    {
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        const string updateSql = @"
UPDATE ""Contratos""
SET
    ""Estado"" = 'Finalizado',
    ""FechaCierre"" = NOW(),
    ""FechaActualizacion"" = NOW()
WHERE ""Id"" = @ContratoId
  AND ""Estado"" = 'Activo';";

        var updated = await conn.ExecuteAsync(updateSql, new { ContratoId = contratoId }, tx);
        if (updated == 0)
        {
            tx.Rollback();
            return false;
        }

        await InsertAuditoriaAsync(conn, tx, contratoId, "Finalizacion", null, new { Estado = "Finalizado" }, observacion, usuarioId);
        tx.Commit();
        return true;
    }

    public async Task<bool> RegistrarReinversionAsync(int contratoId, ReinversionContratoRequest request, int usuarioId)
    {
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        const string getContratoSql = @"
SELECT ""CapitalActual"", ""PorcentajeMensual"" FROM ""Contratos"" WHERE ""Id"" = @ContratoId AND ""Estado"" = 'Activo';";
        var actual = await conn.QuerySingleOrDefaultAsync(getContratoSql, new { ContratoId = contratoId }, tx);
        if (actual is null)
        {
            tx.Rollback();
            return false;
        }

        var capitalActual = (decimal)actual.capitalactual;
        var monto = request.Tipo.Equals("Parcial", StringComparison.OrdinalIgnoreCase)
            ? request.MontoReinvertir ?? 0m
            : capitalActual;

        if (monto <= 0m)
        {
            tx.Rollback();
            return false;
        }

        const string updateSql = @"
UPDATE ""Contratos""
SET ""CapitalActual"" = ""CapitalActual"" + @Monto,
    ""FechaActualizacion"" = NOW()
WHERE ""Id"" = @ContratoId;";

        await conn.ExecuteAsync(updateSql, new { ContratoId = contratoId, Monto = monto }, tx);
        await InsertConfiguracionAsync(conn, tx, contratoId, "Reinversion", monto, null);
        await InsertAuditoriaAsync(conn, tx, contratoId, "Reinversion", null,
            new { Tipo = request.Tipo, Monto = monto }, request.Observacion, usuarioId);

        tx.Commit();
        return true;
    }

    public async Task<bool> RegistrarInyeccionCapitalAsync(int contratoId, InyeccionCapitalRequest request, int usuarioId)
    {
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        const string updateSql = @"
UPDATE ""Contratos""
SET ""CapitalActual"" = ""CapitalActual"" + @Monto,
    ""FechaActualizacion"" = NOW()
WHERE ""Id"" = @ContratoId
  AND ""Estado"" = 'Activo';";

        var updated = await conn.ExecuteAsync(updateSql, new { ContratoId = contratoId, request.Monto }, tx);
        if (updated == 0)
        {
            tx.Rollback();
            return false;
        }

        await InsertConfiguracionAsync(conn, tx, contratoId, "Aporte", request.Monto, null);
        await InsertAuditoriaAsync(conn, tx, contratoId, "InyeccionCapital", null,
            new { request.Monto }, request.Observacion, usuarioId);

        tx.Commit();
        return true;
    }

    public async Task<bool> UnificarContratosAsync(int contratoDestinoId, UnificarContratosRequest request, int usuarioId)
    {
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        var ids = request.ContratosOrigenIds.Distinct().Where(x => x > 0 && x != contratoDestinoId).ToArray();
        if (ids.Length == 0)
        {
            tx.Rollback();
            return false;
        }

        const string destinoSql = @"
SELECT ""ClienteId"", ""CapitalActual""
FROM ""Contratos""
WHERE ""Id"" = @ContratoId
  AND ""Estado"" = 'Activo'
  AND ""PermiteUnificacion"" = TRUE;";

        var destino = await conn.QuerySingleOrDefaultAsync(destinoSql, new { ContratoId = contratoDestinoId }, tx);
        if (destino is null)
        {
            tx.Rollback();
            return false;
        }

        var clienteId = (int)destino.clienteid;
        var capitalAcumulado = (decimal)destino.capitalactual;
        var grupoOperacionId = Guid.NewGuid();

        foreach (var origenId in ids)
        {
            const string origenSql = @"
SELECT ""ClienteId"", ""CapitalActual""
FROM ""Contratos""
WHERE ""Id"" = @ContratoId
  AND ""Estado"" = 'Activo';";

            var origen = await conn.QuerySingleOrDefaultAsync(origenSql, new { ContratoId = origenId }, tx);
            if (origen is null || (int)origen.clienteid != clienteId)
            {
                tx.Rollback();
                return false;
            }

            var monto = (decimal)origen.capitalactual;
            capitalAcumulado += monto;

            const string bloquearOrigenSql = @"
UPDATE ""Contratos""
SET ""Estado"" = 'Unificado',
    ""PermiteUnificacion"" = FALSE,
    ""FechaActualizacion"" = NOW()
WHERE ""Id"" = @ContratoId;";

            await conn.ExecuteAsync(bloquearOrigenSql, new { ContratoId = origenId }, tx);

            const string relacionSql = @"
INSERT INTO ""ContratoRelaciones"" (
    ""ContratoOrigenId"", ""ContratoDestinoId"", ""TipoRelacion"", ""MontoTransferido"",
    ""GrupoOperacionId"", ""Observacion"", ""UsuarioId""
) VALUES (
    @ContratoOrigenId, @ContratoDestinoId, 'Unificacion', @MontoTransferido,
    @GrupoOperacionId, @Observacion, @UsuarioId
);";

            await conn.ExecuteAsync(relacionSql, new
            {
                ContratoOrigenId = origenId,
                ContratoDestinoId = contratoDestinoId,
                MontoTransferido = monto,
                GrupoOperacionId = grupoOperacionId,
                Observacion = request.Observacion,
                UsuarioId = usuarioId
            }, tx);

            await InsertAuditoriaAsync(conn, tx, origenId, "Unificacion", null,
                new { ContratoDestinoId = contratoDestinoId, Monto = monto, grupoOperacionId }, request.Observacion, usuarioId);
        }

        const string updateDestinoSql = @"
UPDATE ""Contratos""
SET ""CapitalActual"" = @CapitalActual,
    ""FechaActualizacion"" = NOW()
WHERE ""Id"" = @ContratoId;";

        await conn.ExecuteAsync(updateDestinoSql, new
        {
            ContratoId = contratoDestinoId,
            CapitalActual = capitalAcumulado
        }, tx);

        await InsertConfiguracionAsync(conn, tx, contratoDestinoId, "Ajuste", capitalAcumulado, null);
        await InsertAuditoriaAsync(conn, tx, contratoDestinoId, "Unificacion", null,
            new { ContratosOrigen = ids, CapitalActual = capitalAcumulado, grupoOperacionId }, request.Observacion, usuarioId);

        tx.Commit();
        return true;
    }

    public async Task<bool> DesunificarContratoAsync(int contratoId, DesunificarContratoRequest request, int usuarioId)
    {
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        const string relacionesSql = @"
SELECT ""Id"", ""ContratoOrigenId"", ""MontoTransferido""
FROM ""ContratoRelaciones""
WHERE ""ContratoDestinoId"" = @ContratoId
  AND ""TipoRelacion"" = 'Unificacion';";

        var relaciones = (await conn.QueryAsync(relacionesSql, new { ContratoId = contratoId }, tx)).ToList();
        if (relaciones.Count == 0)
        {
            tx.Rollback();
            return false;
        }

        foreach (var relacion in relaciones)
        {
            const string revertirOrigenSql = @"
UPDATE ""Contratos""
SET ""Estado"" = 'Activo',
    ""PermiteUnificacion"" = TRUE,
    ""FechaActualizacion"" = NOW()
WHERE ""Id"" = @ContratoOrigenId
  AND ""Estado"" = 'Unificado';";

            await conn.ExecuteAsync(revertirOrigenSql, new { ContratoOrigenId = (int)relacion.contratoorigenid }, tx);

            await InsertAuditoriaAsync(conn, tx, (int)relacion.contratoorigenid, "Desunificacion", null,
                new { ContratoDestinoId = contratoId, Monto = (decimal?)relacion.montotransferido ?? 0m }, request.Observacion, usuarioId);
        }

        const string deleteRelacionesSql = @"
DELETE FROM ""ContratoRelaciones""
WHERE ""ContratoDestinoId"" = @ContratoId
  AND ""TipoRelacion"" = 'Unificacion';";
        await conn.ExecuteAsync(deleteRelacionesSql, new { ContratoId = contratoId }, tx);

        const string recalcularDestinoSql = @"
WITH total_transferido AS (
    SELECT COALESCE(SUM(""MontoTransferido""), 0) AS total
    FROM ""ContratoRelaciones""
    WHERE ""ContratoDestinoId"" = @ContratoId
      AND ""TipoRelacion"" = 'Unificacion'
)
UPDATE ""Contratos""
SET ""CapitalActual"" = GREATEST(""CapitalInicial"", ""CapitalActual"" - (
        SELECT total FROM total_transferido
    )),
    ""FechaActualizacion"" = NOW()
WHERE ""Id"" = @ContratoId;";
        await conn.ExecuteAsync(recalcularDestinoSql, new { ContratoId = contratoId }, tx);

        await InsertAuditoriaAsync(conn, tx, contratoId, "Desunificacion", null,
            new { ContratoId = contratoId }, request.Observacion, usuarioId);

        tx.Commit();
        return true;
    }

    public async Task<IEnumerable<BeneficiarioContratoResponse>> AsignarBeneficiariosAsync(int contratoId, List<AsignarBeneficiarioRequest> beneficiarios, int usuarioId)
    {
        using var conn = _connectionFactory.CreateConnection();
        conn.Open();
        using var tx = conn.BeginTransaction();

        const string deleteSql = @"DELETE FROM ""ContratoBeneficiarios"" WHERE ""ContratoId"" = @ContratoId";
        await conn.ExecuteAsync(deleteSql, new { ContratoId = contratoId }, tx);

        await AsignarBeneficiariosInternalAsync(conn, tx, contratoId, beneficiarios, usuarioId);

        await InsertAuditoriaAsync(conn, tx, contratoId, "CambioBeneficiarios", null,
            new { Beneficiarios = beneficiarios.Count }, "Actualización de beneficiarios", usuarioId);

        tx.Commit();
        return await ObtenerBeneficiariosContratoAsync(contratoId);
    }

    public async Task<IEnumerable<ContratoListaResponse>> ObtenerContratosAsync(ContratoFiltroRequest? filtro)
    {
        const string sql = @"
SELECT
    c.""Id"",
    c.""ClienteId"",
    cl.""NombreCompleto"" AS ""ClienteNombre"",
    c.""NumeroContrato"",
    c.""CapitalInicial"",
    c.""CapitalActual"",
    c.""PorcentajeMensual"",
    c.""ModalidadRendimiento"",
    c.""PermiteUnificacion"",
    c.""Estado"",
    c.""FechaInicio"",
    c.""FechaCreacion"",
    c.""FechaCierre""
FROM ""Contratos"" c
INNER JOIN ""Clientes"" cl ON cl.""Id"" = c.""ClienteId""
WHERE (@ClienteId IS NULL OR c.""ClienteId"" = @ClienteId)
  AND (@Estado IS NULL OR c.""Estado"" = @Estado)
  AND (@NumeroContrato IS NULL OR c.""NumeroContrato"" ILIKE CONCAT('%', @NumeroContrato, '%'))
ORDER BY c.""FechaCreacion"" DESC;";

        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<ContratoListaResponse>(sql, new
        {
            ClienteId = filtro?.ClienteId,
            Estado = filtro?.Estado,
            NumeroContrato = filtro?.NumeroContrato
        });
    }

    public Task<IEnumerable<ContratoListaResponse>> ObtenerContratosClienteAsync(int clienteId)
    {
        return ObtenerContratosAsync(new ContratoFiltroRequest { ClienteId = clienteId });
    }

    public Task<IEnumerable<ContratoListaResponse>> ObtenerContratosActivosClienteAsync(int clienteId)
    {
        return ObtenerContratosAsync(new ContratoFiltroRequest { ClienteId = clienteId, Estado = "Activo" });
    }

    public Task<IEnumerable<ContratoListaResponse>> ObtenerTodosContratosActivosAsync()
    {
        return ObtenerContratosAsync(new ContratoFiltroRequest { Estado = "Activo" });
    }

    public async Task<ContratoResponse?> ObtenerContratoDetalleAsync(int contratoId)
    {
        const string sql = @"
SELECT
    c.""Id"",
    c.""ClienteId"",
    cl.""NombreCompleto"" AS ""ClienteNombre"",
    c.""NumeroContrato"",
    c.""FechaInicio"",
    c.""CapitalInicial"",
    c.""CapitalActual"",
    c.""PorcentajeMensual"",
    c.""ComisionRetiro"",
    c.""ModalidadRendimiento"",
    c.""PermiteUnificacion"",
    c.""Estado"",
    c.""FechaCreacion"",
    c.""FechaActualizacion"",
    c.""FechaCierre""
FROM ""Contratos"" c
INNER JOIN ""Clientes"" cl ON cl.""Id"" = c.""ClienteId""
WHERE c.""Id"" = @ContratoId;";

        using var conn = _connectionFactory.CreateConnection();
        var contrato = await conn.QuerySingleOrDefaultAsync<ContratoResponse>(sql, new { ContratoId = contratoId });
        if (contrato is null)
            return null;

        contrato.Beneficiarios = (await ObtenerBeneficiariosContratoAsync(contratoId)).ToList();
        contrato.Configuraciones = (await conn.QueryAsync<ConfiguracionContratoResponse>(@"
SELECT ""Id"", ""ContratoId"", ""Tipo"", ""CapitalBase"", ""PorcentajeMensual"", ""FechaInicio"", ""FechaFin""
FROM ""ConfiguracionContrato""
WHERE ""ContratoId"" = @ContratoId
ORDER BY ""FechaInicio"" DESC;", new { ContratoId = contratoId })).ToList();

        contrato.Relaciones = (await conn.QueryAsync<ContratoRelacionResponse>(@"
SELECT ""Id"", ""ContratoOrigenId"", ""ContratoDestinoId"", ""TipoRelacion"", ""MontoTransferido"", ""GrupoOperacionId"", ""Observacion"", ""UsuarioId"", ""FechaRelacion""
FROM ""ContratoRelaciones""
WHERE ""ContratoOrigenId"" = @ContratoId OR ""ContratoDestinoId"" = @ContratoId
ORDER BY ""FechaRelacion"" DESC;", new { ContratoId = contratoId })).ToList();

        return contrato;
    }

    public async Task<IEnumerable<BeneficiarioContratoResponse>> ObtenerBeneficiariosContratoAsync(int contratoId)
    {
        const string sql = @"
SELECT
    cb.""Id"",
    cb.""ClienteBeneficiarioId"",
    b.""NombreCompleto"",
    b.""DUI"",
    b.""TipoRelacion"",
    b.""Estado"",
    cb.""PorcentajeAsignado"" AS ""Porcentaje""
FROM ""ContratoBeneficiarios"" cb
INNER JOIN ""ClientesBeneficiarios"" b ON b.""Id"" = cb.""ClienteBeneficiarioId""
WHERE cb.""ContratoId"" = @ContratoId
ORDER BY b.""NombreCompleto"";";

        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<BeneficiarioContratoResponse>(sql, new { ContratoId = contratoId });
    }

    public async Task<IEnumerable<HistorialFinancieroItemResponse>> ObtenerHistorialFinancieroAsync(int contratoId)
    {
        const string sql = @"
SELECT
    cc.""Tipo"" AS ""Tipo"",
    cc.""CapitalBase"" AS ""Monto"",
    cc.""CapitalBase"" AS ""CapitalResultante"",
    cc.""PorcentajeMensual"",
    cc.""FechaInicio""::TIMESTAMP AS ""Fecha"",
    NULL::VARCHAR AS ""Observacion""
FROM ""ConfiguracionContrato"" cc
WHERE cc.""ContratoId"" = @ContratoId
UNION ALL
SELECT
    ac.""TipoMovimiento"" AS ""Tipo"",
    COALESCE((ac.""ValorNuevo"" ->> 'Monto')::NUMERIC, 0) AS ""Monto"",
    0 AS ""CapitalResultante"",
    0 AS ""PorcentajeMensual"",
    ac.""FechaMovimiento"" AS ""Fecha"",
    ac.""Observacion""
FROM ""AuditoriaContratos"" ac
WHERE ac.""ContratoId"" = @ContratoId
ORDER BY ""Fecha"" DESC;";

        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<HistorialFinancieroItemResponse>(sql, new { ContratoId = contratoId });
    }

    public async Task<IEnumerable<ContratoEventoResponse>> ObtenerEventosAsync(int contratoId)
    {
        const string sql = @"
SELECT
    ""Id"",
    ""ContratoId"",
    ""TipoMovimiento"",
    ""Observacion"",
    ""UsuarioId"",
    ""FechaMovimiento""
FROM ""AuditoriaContratos""
WHERE ""ContratoId"" = @ContratoId
ORDER BY ""FechaMovimiento"" DESC;";

        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<ContratoEventoResponse>(sql, new { ContratoId = contratoId });
    }

    public async Task<IEnumerable<AuditoriaContratoResponse>> ObtenerAuditoriaAsync(int contratoId)
    {
        const string sql = @"
SELECT
    ""Id"",
    ""ContratoId"",
    ""TipoMovimiento"",
    ""ValorAnterior""::TEXT AS ""ValorAnterior"",
    ""ValorNuevo""::TEXT AS ""ValorNuevo"",
    ""Observacion"",
    ""UsuarioId"",
    ""FechaMovimiento""
FROM ""AuditoriaContratos""
WHERE ""ContratoId"" = @ContratoId
ORDER BY ""FechaMovimiento"" DESC;";

        using var conn = _connectionFactory.CreateConnection();
        return await conn.QueryAsync<AuditoriaContratoResponse>(sql, new { ContratoId = contratoId });
    }

    private static async Task InsertAuditoriaAsync(
        IDbConnection conn,
        IDbTransaction tx,
        int contratoId,
        string tipoMovimiento,
        object? valorAnterior,
        object? valorNuevo,
        string? observacion,
        int usuarioId)
    {
        const string sql = @"
INSERT INTO ""AuditoriaContratos"" (
    ""ContratoId"", ""TipoMovimiento"", ""ValorAnterior"", ""ValorNuevo"", ""Observacion"", ""UsuarioId""
)
VALUES (
    @ContratoId, @TipoMovimiento, CAST(@ValorAnterior AS JSONB), CAST(@ValorNuevo AS JSONB), @Observacion, @UsuarioId
);";

        await conn.ExecuteAsync(sql, new
        {
            ContratoId = contratoId,
            TipoMovimiento = tipoMovimiento,
            ValorAnterior = valorAnterior is null ? null : System.Text.Json.JsonSerializer.Serialize(valorAnterior),
            ValorNuevo = valorNuevo is null ? null : System.Text.Json.JsonSerializer.Serialize(valorNuevo),
            Observacion = observacion,
            UsuarioId = usuarioId
        }, tx);
    }

    private static async Task InsertConfiguracionAsync(
        IDbConnection conn,
        IDbTransaction tx,
        int contratoId,
        string tipo,
        decimal? capitalBase,
        decimal? porcentajeMensual)
    {
        const string sql = @"
INSERT INTO ""ConfiguracionContrato"" (
    ""ContratoId"", ""Tipo"", ""CapitalBase"", ""PorcentajeMensual"", ""FechaInicio""
)
SELECT
    @ContratoId,
    @Tipo,
    COALESCE(@CapitalBase, c.""CapitalActual""),
    COALESCE(@PorcentajeMensual, c.""PorcentajeMensual""),
    CURRENT_DATE
FROM ""Contratos"" c
WHERE c.""Id"" = @ContratoId;";

        await conn.ExecuteAsync(sql, new
        {
            ContratoId = contratoId,
            Tipo = tipo,
            CapitalBase = capitalBase,
            PorcentajeMensual = porcentajeMensual
        }, tx);
    }

    private static async Task AsignarBeneficiariosInternalAsync(
        IDbConnection conn,
        IDbTransaction tx,
        int contratoId,
        List<AsignarBeneficiarioRequest> beneficiarios,
        int usuarioId)
    {
        if (beneficiarios.Count == 0)
            return;

        var clienteId = await conn.ExecuteScalarAsync<int>(
            @"SELECT ""ClienteId"" FROM ""Contratos"" WHERE ""Id"" = @ContratoId",
            new { ContratoId = contratoId }, tx);

        foreach (var item in beneficiarios)
        {
            int beneficiarioId;
            if (item.ClienteBeneficiarioId.HasValue && item.ClienteBeneficiarioId.Value > 0)
            {
                beneficiarioId = item.ClienteBeneficiarioId.Value;
            }
            else
            {
                const string createBenefSql = @"
INSERT INTO ""ClientesBeneficiarios"" (
    ""ClienteId"", ""NombreCompleto"", ""DUI"", ""Correo"", ""Telefono"", ""Direccion"",
    ""Porcentaje"", ""TipoRelacion"", ""Estado""
)
VALUES (
    @ClienteId, @NombreCompleto, @DUI, @Correo, @Telefono, @Direccion,
    @Porcentaje, @TipoRelacion, 'Activo'
)
RETURNING ""Id"";";

                beneficiarioId = await conn.ExecuteScalarAsync<int>(createBenefSql, new
                {
                    ClienteId = clienteId,
                    NombreCompleto = item.Beneficiario?.NombreCompleto ?? string.Empty,
                    DUI = item.Beneficiario?.DUI,
                    Correo = item.Beneficiario?.Correo,
                    Telefono = item.Beneficiario?.Telefono,
                    Direccion = item.Beneficiario?.Direccion,
                    item.Porcentaje,
                    TipoRelacion = item.Beneficiario?.TipoRelacion ?? "Otro"
                }, tx);
            }

            const string insertSql = @"
INSERT INTO ""ContratoBeneficiarios"" (
    ""ContratoId"", ""ClienteBeneficiarioId"", ""PorcentajeAsignado""
)
VALUES (
    @ContratoId, @ClienteBeneficiarioId, @PorcentajeAsignado
);";

            await conn.ExecuteAsync(insertSql, new
            {
                ContratoId = contratoId,
                ClienteBeneficiarioId = beneficiarioId,
                PorcentajeAsignado = item.Porcentaje
            }, tx);
        }
    }
}

