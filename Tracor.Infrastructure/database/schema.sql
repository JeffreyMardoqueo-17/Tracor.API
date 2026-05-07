CREATE EXTENSION IF NOT EXISTS "pgcrypto";

-- =========================================================
-- LIMPIEZA
-- =========================================================

-- DROP TABLE IF EXISTS "AuditoriaContratos" CASCADE;
-- DROP TABLE IF EXISTS "AtencionRecepcion" CASCADE;
-- DROP TABLE IF EXISTS "Oficinas" CASCADE;
-- DROP TABLE IF EXISTS "PrestamoPagos" CASCADE;
-- DROP TABLE IF EXISTS "Prestamos" CASCADE;
-- DROP TABLE IF EXISTS "AsignacionDetalle" CASCADE;
-- DROP TABLE IF EXISTS "AsignacionesPago" CASCADE;
-- DROP TABLE IF EXISTS "AgendaPagos" CASCADE;
-- DROP TABLE IF EXISTS "DecisionesPago" CASCADE;
-- DROP TABLE IF EXISTS "Pagos" CASCADE;
-- DROP TABLE IF EXISTS "MovimientosContrato" CASCADE;
-- DROP TABLE IF EXISTS "CalculoPagos" CASCADE;
-- DROP TABLE IF EXISTS "BloquesPago" CASCADE;
-- DROP TABLE IF EXISTS "ReglasPago" CASCADE;
-- DROP TABLE IF EXISTS "PlanesPago" CASCADE;
-- DROP TABLE IF EXISTS "CiclosPago" CASCADE;
-- DROP TABLE IF EXISTS "FechasCortePago" CASCADE;
-- DROP TABLE IF EXISTS "ConfiguracionCortesPago" CASCADE;
-- DROP TABLE IF EXISTS "ConfiguracionSistema" CASCADE;
-- DROP TABLE IF EXISTS "ConfiguracionContrato" CASCADE;
-- DROP TABLE IF EXISTS "ContratoBeneficiarios" CASCADE;
-- DROP TABLE IF EXISTS "ContratoRelaciones" CASCADE;
-- DROP TABLE IF EXISTS "Contratos" CASCADE;
-- DROP TABLE IF EXISTS "ClienteCuentas" CASCADE;
-- DROP TABLE IF EXISTS "ClienteDirecciones" CASCADE;
-- DROP TABLE IF EXISTS "ClientesBeneficiariosHistorico" CASCADE;
-- DROP TABLE IF EXISTS "ClientesBeneficiarios" CASCADE;
-- DROP TABLE IF EXISTS "Clientes" CASCADE;
-- DROP TABLE IF EXISTS "Pais" CASCADE;
-- DROP TABLE IF EXISTS "Bancos" CASCADE;

-- =========================================================
-- USUARIOS
-- =========================================================
-- NO SE MODIFICA

CREATE TABLE "Usuarios" (
    "Id" SERIAL PRIMARY KEY,
    "Nombre" VARCHAR(150) NOT NULL,
    "Email" VARCHAR(150) UNIQUE NOT NULL,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "Rol" VARCHAR(50) NOT NULL CHECK ("Rol" IN ('Admin', 'Ejecutivo')),
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,
    "FechaCreacion" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- =========================================================
-- BANCOS
-- =========================================================

CREATE TABLE "Bancos" (
    "Id" SERIAL PRIMARY KEY,
    "Nombre" VARCHAR(150) UNIQUE NOT NULL,
    "Codigo" VARCHAR(20),
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE
);

-- =========================================================
-- PAISES
-- =========================================================

CREATE TABLE "Pais" (
    "Id" SERIAL PRIMARY KEY,
    "Nombre" VARCHAR(100) UNIQUE NOT NULL
);

-- =========================================================
-- CLIENTES
-- =========================================================

CREATE TABLE "Clientes" (
    "Id" SERIAL PRIMARY KEY,
    "CodigoCliente" VARCHAR(50) UNIQUE NOT NULL,
    "NombreCompleto" VARCHAR(250) NOT NULL,
    "TipoDocumento" VARCHAR(50),
    "NumeroDocumento" VARCHAR(100) UNIQUE,
    "TipoPersona" VARCHAR(20) NOT NULL CHECK ("TipoPersona" IN ('Natural', 'Juridica')),
    "Correo" VARCHAR(150),
    "Telefono" VARCHAR(50),
    "Notas" VARCHAR(500),
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,
    "UsuarioEjecutivoId" INT NOT NULL,
    "FechaCreacion" TIMESTAMP NOT NULL DEFAULT NOW(),

    FOREIGN KEY ("UsuarioEjecutivoId")
        REFERENCES "Usuarios"("Id")
);

CREATE INDEX "IX_Clientes_Codigo"
ON "Clientes"("CodigoCliente");

CREATE INDEX "IX_Clientes_Documento"
ON "Clientes"("NumeroDocumento");

-- =========================================================
-- DIRECCIONES CLIENTE
-- =========================================================

CREATE TABLE "ClienteDirecciones" (
    "Id" SERIAL PRIMARY KEY,
    "ClienteId" INT NOT NULL,
    "PaisId" INT NOT NULL,
    "Direccion" VARCHAR(500) NOT NULL,
    "Referencia" VARCHAR(500),
    "EsPrincipal" BOOLEAN NOT NULL DEFAULT TRUE,

    FOREIGN KEY ("ClienteId")
        REFERENCES "Clientes"("Id")
        ON DELETE CASCADE,

    FOREIGN KEY ("PaisId")
        REFERENCES "Pais"("Id")
);

-- =========================================================
-- CUENTAS CLIENTE
-- =========================================================

CREATE TABLE "ClienteCuentas" (
    "Id" SERIAL PRIMARY KEY,
    "ClienteId" INT NOT NULL,
    "BancoId" INT NOT NULL,
    "NumeroCuenta" VARCHAR(150) NOT NULL,
    "TipoCuenta" VARCHAR(50),
    "Titular" VARCHAR(250),
    "EsPrincipal" BOOLEAN NOT NULL DEFAULT FALSE,
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,
    "FechaCreacion" TIMESTAMP NOT NULL DEFAULT NOW(),

    FOREIGN KEY ("ClienteId")
        REFERENCES "Clientes"("Id"),

    FOREIGN KEY ("BancoId")
        REFERENCES "Bancos"("Id")
);

CREATE UNIQUE INDEX "UX_Cliente_CuentaPrincipal"
ON "ClienteCuentas"("ClienteId")
WHERE "EsPrincipal" = TRUE;

-- =========================================================
-- BENEFICIARIOS
-- =========================================================

CREATE TABLE "ClientesBeneficiarios" (
    "Id" SERIAL PRIMARY KEY,
    "ClienteId" INT NOT NULL,
    "NombreCompleto" VARCHAR(250) NOT NULL,
    "DUI" VARCHAR(50),
    "Correo" VARCHAR(150),
    "Telefono" VARCHAR(50),
    "Direccion" VARCHAR(500),

    "Porcentaje" NUMERIC(5,2) NOT NULL
        CHECK ("Porcentaje" >= 0 AND "Porcentaje" <= 100),

    "TipoRelacion" VARCHAR(50) NOT NULL,

    "Estado" VARCHAR(50) NOT NULL
        CHECK ("Estado" IN ('Activo', 'Inactivo', 'Fallecido')),

    "FechaCreacion" TIMESTAMP NOT NULL DEFAULT NOW(),

    FOREIGN KEY ("ClienteId")
        REFERENCES "Clientes"("Id")
        ON DELETE CASCADE
);

CREATE TABLE "ClientesBeneficiariosHistorico" (
    "Id" SERIAL PRIMARY KEY,
    "ClienteBeneficiarioId" INT NOT NULL,
    "ClienteId" INT NOT NULL,
    "Evento" VARCHAR(200) NOT NULL,
    "Notas" VARCHAR(500),
    "FechaEvento" TIMESTAMP NOT NULL DEFAULT NOW(),

    FOREIGN KEY ("ClienteBeneficiarioId")
        REFERENCES "ClientesBeneficiarios"("Id"),

    FOREIGN KEY ("ClienteId")
        REFERENCES "Clientes"("Id")
);

-- =========================================================
-- CONTRATOS
-- =========================================================

CREATE TABLE "Contratos" (
    "Id" SERIAL PRIMARY KEY,

    "ClienteId" INT NOT NULL,

    "NumeroContrato" VARCHAR(100) UNIQUE NOT NULL,

    "FechaInicio" DATE NOT NULL,

    "CapitalInicial" NUMERIC(18,2) NOT NULL
        CHECK ("CapitalInicial" > 0),

    "CapitalActual" NUMERIC(18,2) NOT NULL
        CHECK ("CapitalActual" >= 0),

    "PorcentajeMensual" NUMERIC(5,2) NOT NULL
        CHECK ("PorcentajeMensual" >= 0),

    "ComisionRetiro" NUMERIC(5,2) NOT NULL DEFAULT 0,

    "ModalidadRendimiento" VARCHAR(50) NOT NULL
        CHECK ("ModalidadRendimiento" IN ('Normal', 'InteresCompuesto')),

    "Estado" VARCHAR(50) NOT NULL
        CHECK ("Estado" IN (
            'Activo',
            'Finalizado',
            'Unificado',
            'Anulado'
        )),

    "PermiteUnificacion" BOOLEAN NOT NULL DEFAULT TRUE,

    "FechaCreacion" TIMESTAMP NOT NULL DEFAULT NOW(),
    "FechaActualizacion" TIMESTAMP NOT NULL DEFAULT NOW(),
    "FechaCierre" TIMESTAMP,

    FOREIGN KEY ("ClienteId")
        REFERENCES "Clientes"("Id")
);

CREATE INDEX "IX_Contratos_Cliente"
ON "Contratos"("ClienteId");

CREATE INDEX "IX_Contratos_Estado"
ON "Contratos"("Estado");

-- =========================================================
-- BENEFICIARIOS POR CONTRATO
-- =========================================================

CREATE TABLE "ContratoBeneficiarios" (
    "Id" SERIAL PRIMARY KEY,

    "ContratoId" INT NOT NULL,
    "ClienteBeneficiarioId" INT NOT NULL,

    "PorcentajeAsignado" NUMERIC(5,2) NOT NULL
        CHECK ("PorcentajeAsignado" >= 0 AND "PorcentajeAsignado" <= 100),

    FOREIGN KEY ("ContratoId")
        REFERENCES "Contratos"("Id")
        ON DELETE CASCADE,

    FOREIGN KEY ("ClienteBeneficiarioId")
        REFERENCES "ClientesBeneficiarios"("Id")
);

-- =========================================================
-- RELACIONES CONTRATOS
-- =========================================================

CREATE TABLE "ContratoRelaciones" (
    "Id" SERIAL PRIMARY KEY,

    "ContratoOrigenId" INT NOT NULL,
    "ContratoDestinoId" INT NOT NULL,

    "TipoRelacion" VARCHAR(50) NOT NULL
        CHECK ("TipoRelacion" IN (
            'Unificacion',
            'Desunificacion',
            'InyeccionCapital',
            'Reinversion'
        )),

    "MontoTransferido" NUMERIC(18,2),

    "GrupoOperacionId" UUID NOT NULL DEFAULT gen_random_uuid(),

    "Observacion" VARCHAR(500),

    "UsuarioId" INT NOT NULL,

    "FechaRelacion" TIMESTAMP NOT NULL DEFAULT NOW(),

    FOREIGN KEY ("ContratoOrigenId")
        REFERENCES "Contratos"("Id"),

    FOREIGN KEY ("ContratoDestinoId")
        REFERENCES "Contratos"("Id"),

    FOREIGN KEY ("UsuarioId")
        REFERENCES "Usuarios"("Id")
);

-- =========================================================
-- CONFIGURACIONES CONTRATO
-- =========================================================

CREATE TABLE "ConfiguracionContrato" (
    "Id" SERIAL PRIMARY KEY,

    "ContratoId" INT NOT NULL,

    "Tipo" VARCHAR(50) NOT NULL
        CHECK ("Tipo" IN (
            'Inicial',
            'Aporte',
            'Retiro',
            'Reinversion',
            'Ajuste'
        )),

    "CapitalBase" NUMERIC(18,2) NOT NULL,
    "PorcentajeMensual" NUMERIC(5,2) NOT NULL,

    "FechaInicio" DATE NOT NULL,
    "FechaFin" DATE,

    FOREIGN KEY ("ContratoId")
        REFERENCES "Contratos"("Id")
        ON DELETE CASCADE
);

-- =========================================================
-- AUDITORIA CONTRATOS
-- =========================================================

CREATE TABLE "AuditoriaContratos" (
    "Id" BIGSERIAL PRIMARY KEY,

    "ContratoId" INT NOT NULL,

    "TipoMovimiento" VARCHAR(100) NOT NULL
        CHECK ("TipoMovimiento" IN (
            'Creacion',
            'Actualizacion',
            'Reinversion',
            'CambioPorcentaje',
            'InyeccionCapital',
            'Unificacion',
            'Desunificacion',
            'Finalizacion',
            'CambioEstado',
            'CambioBeneficiarios',
            'CambioCapital'
        )),

    "ValorAnterior" JSONB,
    "ValorNuevo" JSONB,

    "Observacion" VARCHAR(1000),

    "UsuarioId" INT NOT NULL,

    "FechaMovimiento" TIMESTAMP NOT NULL DEFAULT NOW(),

    FOREIGN KEY ("ContratoId")
        REFERENCES "Contratos"("Id"),

    FOREIGN KEY ("UsuarioId")
        REFERENCES "Usuarios"("Id")
);

CREATE INDEX "IX_AuditoriaContrato_Contrato"
ON "AuditoriaContratos"("ContratoId");



-- =========================================================
-- CONFIGURACION CORTES
-- =========================================================

CREATE TABLE "ConfiguracionCortesPago" (
    "Id" SERIAL PRIMARY KEY,

    "Nombre" VARCHAR(100) NOT NULL,

    "Activo" BOOLEAN NOT NULL DEFAULT TRUE
);

CREATE TABLE "FechasCortePago" (
    "Id" SERIAL PRIMARY KEY,

    "ConfiguracionCortePagoId" INT NOT NULL,

    "Orden" INT NOT NULL,
    "Dia" INT NOT NULL,
    "Mes" INT NOT NULL,

    FOREIGN KEY ("ConfiguracionCortePagoId")
        REFERENCES "ConfiguracionCortesPago"("Id")
);

-- =========================================================
-- CICLOS
-- =========================================================

CREATE TABLE "CiclosPago" (
    "Id" SERIAL PRIMARY KEY,

    "Nombre" VARCHAR(100) NOT NULL,

    "FechaInicio" DATE NOT NULL,
    "FechaFin" DATE NOT NULL,

    "Estado" VARCHAR(50) NOT NULL
        CHECK ("Estado" IN (
            'Planificacion',
            'EnProceso',
            'Finalizado'
        ))
);

-- =========================================================
-- PLANES PAGO
-- =========================================================

CREATE TABLE "PlanesPago" (
    "Id" SERIAL PRIMARY KEY,

    "CicloPagoId" INT NOT NULL,

    "Nombre" VARCHAR(100) NOT NULL,

    FOREIGN KEY ("CicloPagoId")
        REFERENCES "CiclosPago"("Id")
);

-- =========================================================
-- BLOQUES PAGO
-- =========================================================

CREATE TABLE "BloquesPago" (
    "Id" SERIAL PRIMARY KEY,

    "PlanPagoId" INT NOT NULL,

    "Orden" INT NOT NULL,

    "MontoMinimo" NUMERIC(18,2) NOT NULL,
    "MontoMaximo" NUMERIC(18,2) NOT NULL,

    "PermiteAgendamiento" BOOLEAN NOT NULL DEFAULT TRUE,

    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,

    FOREIGN KEY ("PlanPagoId")
        REFERENCES "PlanesPago"("Id")
);

-- =========================================================
-- REGLAS PAGO
-- =========================================================

CREATE TABLE "ReglasPago" (
    "Id" SERIAL PRIMARY KEY,

    "PlanPagoId" INT NOT NULL,

    "FechaPago" DATE NOT NULL,

    "TipoPago" VARCHAR(50)
        CHECK ("TipoPago" IN (
            'Transferencia',
            'Efectivo',
            'Ambos'
        )),

    FOREIGN KEY ("PlanPagoId")
        REFERENCES "PlanesPago"("Id")
);

-- =========================================================
-- PRESTAMOS
-- =========================================================

CREATE TABLE "Prestamos" (
    "Id" SERIAL PRIMARY KEY,

    "ContratoId" INT NOT NULL,

    "EmpresaOrigen" VARCHAR(150) NOT NULL,

    "MontoPrestado" NUMERIC(18,2) NOT NULL,

    "SaldoPendiente" NUMERIC(18,2) NOT NULL,

    "CuotaMensual" NUMERIC(18,2),

    "Estado" VARCHAR(50) NOT NULL
        CHECK ("Estado" IN (
            'Activo',
            'Pagado',
            'Cancelado'
        )),

    "FechaInicio" DATE NOT NULL,

    "FechaCreacion" TIMESTAMP NOT NULL DEFAULT NOW(),

    FOREIGN KEY ("ContratoId")
        REFERENCES "Contratos"("Id")
);

CREATE INDEX "IX_Prestamos_Contrato"
ON "Prestamos"("ContratoId");

-- =========================================================
-- CALCULOS
-- =========================================================

CREATE TABLE "CalculoPagos" (
    "Id" SERIAL PRIMARY KEY,

    "ContratoId" INT NOT NULL,
    "CicloPagoId" INT NOT NULL,

    "BloquePagoId" INT,

    "FechaCorte" DATE NOT NULL,

    "DiasCalculados" INT NOT NULL,

    "CapitalCalculado" NUMERIC(18,2) NOT NULL,

    "PorcentajeAplicado" NUMERIC(5,2) NOT NULL,

    "MontoGanancia" NUMERIC(18,2) NOT NULL,

    "MontoComision" NUMERIC(18,2) NOT NULL DEFAULT 0,

    "MontoFinal" NUMERIC(18,2) NOT NULL,

    "Estado" VARCHAR(50) NOT NULL
        CHECK ("Estado" IN (
            'Pendiente',
            'Procesado',
            'Pagado',
            'AplicadoPrestamo',
            'Anulado'
        )),

    FOREIGN KEY ("ContratoId")
        REFERENCES "Contratos"("Id"),

    FOREIGN KEY ("CicloPagoId")
        REFERENCES "CiclosPago"("Id"),

    FOREIGN KEY ("BloquePagoId")
        REFERENCES "BloquesPago"("Id")
);

-- =========================================================
-- PAGOS
-- =========================================================

CREATE TABLE "Pagos" (
    "Id" SERIAL PRIMARY KEY,

    "CalculoPagoId" INT NOT NULL UNIQUE,

    "ContratoId" INT NOT NULL,
    "ClienteId" INT NOT NULL,
    "CicloPagoId" INT NOT NULL,

    "MetodoPago" VARCHAR(50)
        CHECK ("MetodoPago" IN (
            'Transferencia',
            'Efectivo',
            'Mixto'
        )),

    "Estado" VARCHAR(50) NOT NULL
        CHECK ("Estado" IN (
            'Pendiente',
            'Agendado',
            'Pagado',
            'AplicadoPrestamo',
            'Error',
            'Anulado'
        )),

    "GananciaBruta" NUMERIC(18,2) NOT NULL,
    "ComisionAplicada" NUMERIC(18,2) NOT NULL DEFAULT 0,
    "MontoEntregado" NUMERIC(18,2) NOT NULL,

    "FechaPago" TIMESTAMP,

    "EjecutadoPor" INT,

    "ClienteCuentaId" INT,
    "BancoId" INT,

    "Observacion" VARCHAR(500),

    FOREIGN KEY ("CalculoPagoId")
        REFERENCES "CalculoPagos"("Id"),

    FOREIGN KEY ("ContratoId")
        REFERENCES "Contratos"("Id"),

    FOREIGN KEY ("ClienteId")
        REFERENCES "Clientes"("Id"),

    FOREIGN KEY ("CicloPagoId")
        REFERENCES "CiclosPago"("Id"),

    FOREIGN KEY ("EjecutadoPor")
        REFERENCES "Usuarios"("Id"),

    FOREIGN KEY ("ClienteCuentaId")
        REFERENCES "ClienteCuentas"("Id"),

    FOREIGN KEY ("BancoId")
        REFERENCES "Bancos"("Id")
);

-- =========================================================
-- PAGOS PRESTAMOS
-- =========================================================

CREATE TABLE "PrestamoPagos" (
    "Id" SERIAL PRIMARY KEY,

    "PrestamoId" INT NOT NULL,
    "PagoId" INT NOT NULL,

    "MontoAplicado" NUMERIC(18,2) NOT NULL,

    "FechaAplicacion" TIMESTAMP NOT NULL DEFAULT NOW(),

    FOREIGN KEY ("PrestamoId")
        REFERENCES "Prestamos"("Id"),

    FOREIGN KEY ("PagoId")
        REFERENCES "Pagos"("Id")
);

-- =========================================================
-- DECISIONES PAGO
-- =========================================================

CREATE TABLE "DecisionesPago" (
    "Id" SERIAL PRIMARY KEY,

    "PagoId" INT NOT NULL,

    "TipoDecision" VARCHAR(50)
        CHECK ("TipoDecision" IN (
            'RetiroTotal',
            'ReinversionTotal',
            'ReinversionParcial',
            'InteresCompuesto'
        )),

    "MetodoRetiro" VARCHAR(50)
        CHECK ("MetodoRetiro" IN (
            'Transferencia',
            'Efectivo',
            'Mixto'
        )),

    "MontoRetirado" NUMERIC(18,2) DEFAULT 0,
    "MontoReinvertido" NUMERIC(18,2) DEFAULT 0,

    "ClienteCuentaId" INT,

    "FechaDecision" TIMESTAMP NOT NULL DEFAULT NOW(),

    FOREIGN KEY ("PagoId")
        REFERENCES "Pagos"("Id"),

    FOREIGN KEY ("ClienteCuentaId")
        REFERENCES "ClienteCuentas"("Id")
);

-- =========================================================
-- OFICINAS
-- =========================================================

CREATE TABLE "Oficinas" (
    "Id" SERIAL PRIMARY KEY,

    "Nombre" VARCHAR(100) NOT NULL,

    "Estado" VARCHAR(50) NOT NULL
        CHECK ("Estado" IN (
            'Disponible',
            'Ocupada',
            'Inactiva'
        ))
);

-- =========================================================
-- AGENDA PAGOS
-- =========================================================

CREATE TABLE "AgendaPagos" (
    "Id" SERIAL PRIMARY KEY,

    "PagoId" INT NOT NULL,

    "ClienteId" INT NOT NULL,

    "FechaProgramada" TIMESTAMP NOT NULL,

    "Tipo" VARCHAR(50)
        CHECK ("Tipo" IN (
            'Oficina',
            'Transferencia'
        )),

    "Estado" VARCHAR(50)
        CHECK ("Estado" IN (
            'Programado',
            'EnEspera',
            'EnProceso',
            'Atendido',
            'NoAsistio'
        )),

    "OficinaId" INT,

    FOREIGN KEY ("PagoId")
        REFERENCES "Pagos"("Id"),

    FOREIGN KEY ("ClienteId")
        REFERENCES "Clientes"("Id"),

    FOREIGN KEY ("OficinaId")
        REFERENCES "Oficinas"("Id")
);

-- =========================================================
-- RECEPCION
-- =========================================================

CREATE TABLE "AtencionRecepcion" (
    "Id" SERIAL PRIMARY KEY,

    "AgendaPagoId" INT NOT NULL,

    "ClienteId" INT NOT NULL,

    "OficinaId" INT,

    "Estado" VARCHAR(50) NOT NULL
        CHECK ("Estado" IN (
            'EnSala',
            'EnProceso',
            'Atendido'
        )),

    "HoraLlegada" TIMESTAMP NOT NULL DEFAULT NOW(),
    "HoraAtencion" TIMESTAMP,
    "HoraFinalizacion" TIMESTAMP,

    "ResponsableId" INT,

    FOREIGN KEY ("AgendaPagoId")
        REFERENCES "AgendaPagos"("Id"),

    FOREIGN KEY ("ClienteId")
        REFERENCES "Clientes"("Id"),

    FOREIGN KEY ("OficinaId")
        REFERENCES "Oficinas"("Id"),

    FOREIGN KEY ("ResponsableId")
        REFERENCES "Usuarios"("Id")
);

-- =========================================================
-- ASIGNACIONES
-- =========================================================

CREATE TABLE "AsignacionesPago" (
    "Id" SERIAL PRIMARY KEY,

    "UsuarioId" INT NOT NULL,

    "CicloPagoId" INT NOT NULL,

    "FechaAsignacion" DATE NOT NULL DEFAULT CURRENT_DATE,

    "Jornada" VARCHAR(50)
        CHECK ("Jornada" IN (
            'Manana',
            'Tarde'
        )),

    "BancoId" INT,

    "TipoPago" VARCHAR(50)
        CHECK ("TipoPago" IN (
            'Transferencia',
            'Efectivo',
            'Ambos'
        )),

    FOREIGN KEY ("UsuarioId")
        REFERENCES "Usuarios"("Id"),

    FOREIGN KEY ("CicloPagoId")
        REFERENCES "CiclosPago"("Id"),

    FOREIGN KEY ("BancoId")
        REFERENCES "Bancos"("Id")
);

CREATE TABLE "AsignacionDetalle" (
    "Id" SERIAL PRIMARY KEY,

    "AsignacionId" INT NOT NULL,
    "PagoId" INT NOT NULL UNIQUE,

    FOREIGN KEY ("AsignacionId")
        REFERENCES "AsignacionesPago"("Id"),

    FOREIGN KEY ("PagoId")
        REFERENCES "Pagos"("Id")
);
