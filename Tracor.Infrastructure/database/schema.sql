CREATE EXTENSION IF NOT EXISTS "pgcrypto";

DROP TABLE IF EXISTS "AsignacionDetalle" CASCADE;
DROP TABLE IF EXISTS "AsignacionesPago" CASCADE;
DROP TABLE IF EXISTS "AgendaPagos" CASCADE;
DROP TABLE IF EXISTS "DecisionesPago" CASCADE;
DROP TABLE IF EXISTS "Pagos" CASCADE;
DROP TABLE IF EXISTS "MovimientosContrato" CASCADE;
DROP TABLE IF EXISTS "CalculoPagos" CASCADE;
DROP TABLE IF EXISTS "ReglasPago" CASCADE;
DROP TABLE IF EXISTS "PlanesPago" CASCADE;
DROP TABLE IF EXISTS "CiclosPago" CASCADE;
DROP TABLE IF EXISTS "FechasCortePago" CASCADE;
DROP TABLE IF EXISTS "ConfiguracionCortesPago" CASCADE;
DROP TABLE IF EXISTS "ConfiguracionSistema" CASCADE;
DROP TABLE IF EXISTS "ConfiguracionContrato" CASCADE;
DROP TABLE IF EXISTS "ContratoRelaciones" CASCADE;
DROP TABLE IF EXISTS "Contratos" CASCADE;
DROP TABLE IF EXISTS "ClienteCuentas" CASCADE;
DROP TABLE IF EXISTS "ClientesBeneficiariosHistorico" CASCADE;
DROP TABLE IF EXISTS "ClientesBeneficiarios" CASCADE;
DROP TABLE IF EXISTS "Clientes" CASCADE;
DROP TABLE IF EXISTS "Bancos" CASCADE;
DROP TABLE IF EXISTS "Usuarios" CASCADE;

CREATE TABLE "Usuarios" (
    "Id" SERIAL PRIMARY KEY,
    "Nombre" VARCHAR(150) NOT NULL,
    "Email" VARCHAR(150) UNIQUE NOT NULL,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "Rol" VARCHAR(50) NOT NULL CHECK ("Rol" IN ('Admin', 'Ejecutivo')),
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,
    "FechaCreacion" TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE "Bancos" (
    "Id" SERIAL PRIMARY KEY,
    "Nombre" VARCHAR(100) UNIQUE NOT NULL
);

CREATE TABLE "Clientes" (
    "Id" SERIAL PRIMARY KEY,
    "CodigoCliente" VARCHAR(50) UNIQUE NOT NULL,
    "NombreCompleto" VARCHAR(200) NOT NULL,
    "TipoDocumento" VARCHAR(50),
    "NumeroDocumento" VARCHAR(50) UNIQUE,
    "TipoPersona" VARCHAR(50),
    "UsuarioEjecutivoId" INT NOT NULL,
    "Correo" VARCHAR(150),
    "Telefono" VARCHAR(50),
    "Notas" VARCHAR(500),
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,
    FOREIGN KEY ("UsuarioEjecutivoId") REFERENCES "Usuarios"("Id")
);
-- ////////este de aqui es u parche para mejorar las consultas en el futuro, se puede eliminar despues
CREATE TABLE Pais (
    "Id" SERIAL PRIMARY KEY,
    "Nombre" VARCHAR(100) UNIQUE NOT NULL
);

-- tabla para guardar la direccion del cliente
CREATE TABLE "ClienteDirecciones" (
    "Id" SERIAL PRIMARY KEY,
    "IdPais" INT NOT NULL,
    "ClienteId" INT NOT NULL,
    "Direccion" VARCHAR(500) NOT NULL,
    FOREIGN KEY ("ClienteId") REFERENCES "Clientes"("Id") ON DELETE CASCADE,
    FOREIGN KEY ("IdPais") REFERENCES "Pais"("Id")
);
-- ////////////////////fin del parche
CREATE TABLE "ClientesBeneficiarios" (
    "Id" SERIAL PRIMARY KEY,
    "ClienteId" INT NOT NULL,
    "NombreCompleto" VARCHAR(200) NOT NULL,
    "DUI" VARCHAR(50),
    "Correo" VARCHAR(150),
    "Telefono" VARCHAR(50),
    "Direccion" VARCHAR(500),
    "Porcentaje" NUMERIC(5,2) NOT NULL CHECK ("Porcentaje" >= 0 AND "Porcentaje" <= 100),
    "TipoRelacion" VARCHAR(50) NOT NULL CHECK ("TipoRelacion" IN ('Conyuge', 'Hijo', 'Hija', 'Padre', 'Madre', 'Hermano', 'Hermana', 'Abuelo', 'Abuela', 'Nieto', 'Nieta', 'Tio', 'Tia', 'Primo', 'Prima', 'Sobrino', 'Sobrina', 'Otro')),
    "Estado" VARCHAR(50) NOT NULL CHECK ("Estado" IN ('Activo', 'Inactivo', 'Fallecido')),
    "Notas" VARCHAR(500),
    "FechaCreacion" TIMESTAMP NOT NULL DEFAULT NOW(),
    "FechaActualizacion" TIMESTAMP NOT NULL DEFAULT NOW(),
    -- "FechaFallecimiento" TIMESTAMP,
    FOREIGN KEY ("ClienteId") REFERENCES "Clientes"("Id") ON DELETE CASCADE
);

CREATE INDEX "IX_ClientesBeneficiarios_ClienteId" ON "ClientesBeneficiarios" ("ClienteId");
CREATE INDEX "IX_ClientesBeneficiarios_Estado" ON "ClientesBeneficiarios" ("ClienteId", "Estado") WHERE "Estado" = 'Activo';

CREATE TABLE "ClientesBeneficiariosHistorico" (
    "Id" SERIAL PRIMARY KEY,
    "ClienteBeneficiarioId" INT NOT NULL,
    "ClienteId" INT NOT NULL,
    "NombreCompleto" VARCHAR(200) NOT NULL,
    "DUI" VARCHAR(50),
    "PorcentajeAsignado" NUMERIC(5,2) NOT NULL CHECK ("PorcentajeAsignado" >= 0 AND "PorcentajeAsignado" <= 100),
    "TipoRelacion" VARCHAR(50) NOT NULL,
    "Evento" VARCHAR(200) NOT NULL,
    "FechaEvento" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Notas" VARCHAR(500),
    FOREIGN KEY ("ClienteBeneficiarioId") REFERENCES "ClientesBeneficiarios"("Id"),
    FOREIGN KEY ("ClienteId") REFERENCES "Clientes"("Id")
);

CREATE INDEX "IX_ClientesBeneficiariosHistorico_ClienteId" ON "ClientesBeneficiariosHistorico" ("ClienteId");
CREATE INDEX "IX_ClientesBeneficiariosHistorico_BeneficiarioId" ON "ClientesBeneficiariosHistorico" ("ClienteBeneficiarioId");

CREATE TABLE "ClienteCuentas" (
    "Id" SERIAL PRIMARY KEY,
    "ClienteId" INT NOT NULL,
    "BancoId" INT NOT NULL,
    "NumeroCuenta" VARCHAR(100) NOT NULL,
    "TipoCuenta" VARCHAR(50),
    "EsPrincipal" BOOLEAN NOT NULL DEFAULT FALSE,
    FOREIGN KEY ("ClienteId") REFERENCES "Clientes"("Id"),
    FOREIGN KEY ("BancoId") REFERENCES "Bancos"("Id")
);

CREATE UNIQUE INDEX "UX_CuentaPrincipal"
ON "ClienteCuentas" ("ClienteId")
WHERE "EsPrincipal" = TRUE;

CREATE TABLE "Contratos" (
    "Id" SERIAL PRIMARY KEY,
    "ClienteId" INT NOT NULL,
    "NumeroContrato" VARCHAR(50) UNIQUE NOT NULL,
    "FechaInicio" DATE NOT NULL,
    "CapitalInicial" NUMERIC(18,2) NOT NULL CHECK ("CapitalInicial" > 0),
    "PorcentajeMensual" NUMERIC(5,2) NOT NULL CHECK ("PorcentajeMensual" > 0),
    "ComisionRetiro" NUMERIC(5,2) NOT NULL DEFAULT 0 CHECK ("ComisionRetiro" >= 0),
    "ModalidadRendimiento" VARCHAR(50) NOT NULL CHECK ("ModalidadRendimiento" IN ('Normal', 'InteresCompuesto')),
    "PermiteUnificacion" BOOLEAN NOT NULL DEFAULT TRUE,
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,
    "FechaCreacion" TIMESTAMP NOT NULL DEFAULT NOW(),
    "FechaCierre" TIMESTAMP,
    FOREIGN KEY ("ClienteId") REFERENCES "Clientes"("Id")
);

CREATE TABLE "ContratoRelaciones" (
    "Id" SERIAL PRIMARY KEY,
    "ContratoOrigenId" INT NOT NULL,
    "ContratoDestinoId" INT NOT NULL,
    "TipoRelacion" VARCHAR(50) NOT NULL CHECK ("TipoRelacion" IN ('Unificacion', 'Desunificacion')),
    "FechaRelacion" TIMESTAMP NOT NULL DEFAULT NOW(),
    "MontoTransferido" NUMERIC(18,2),
    "Observacion" VARCHAR(500),
    "GrupoOperacionId" UUID NOT NULL DEFAULT gen_random_uuid(),
    "UsuarioId" INT,
    FOREIGN KEY ("ContratoOrigenId") REFERENCES "Contratos"("Id"),
    FOREIGN KEY ("ContratoDestinoId") REFERENCES "Contratos"("Id"),
    FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios"("Id")
);

CREATE INDEX "IX_ContratoRelaciones_Operacion"
ON "ContratoRelaciones" ("ContratoOrigenId", "ContratoDestinoId", "FechaRelacion");

CREATE TABLE "ConfiguracionContrato" (
    "Id" SERIAL PRIMARY KEY,
    "ContratoId" INT NOT NULL,
    "FechaInicio" DATE NOT NULL,
    "FechaFin" DATE,
    "CapitalBase" NUMERIC(18,2) NOT NULL CHECK ("CapitalBase" >= 0),
    "PorcentajeMensual" NUMERIC(5,2) NOT NULL CHECK ("PorcentajeMensual" >= 0),
    "Tipo" VARCHAR(50) NOT NULL CHECK ("Tipo" IN ('Inicial', 'Reinversion', 'Aporte', 'Retiro', 'Ajuste')),
    FOREIGN KEY ("ContratoId") REFERENCES "Contratos"("Id")
);

CREATE INDEX "IX_ConfigContrato"
ON "ConfiguracionContrato" ("ContratoId", "FechaInicio");

CREATE TABLE "ConfiguracionSistema" (
    "Id" SERIAL PRIMARY KEY,
    "DiasCuatrimestreBase" INT NOT NULL DEFAULT 120 CHECK ("DiasCuatrimestreBase" > 0),
    "DiasMesBase" INT NOT NULL DEFAULT 30 CHECK ("DiasMesBase" > 0),
    "ComisionEmpresaPorcentaje" NUMERIC(5,2) NOT NULL DEFAULT 5 CHECK ("ComisionEmpresaPorcentaje" >= 0),
    "UsarDiasExactosPrimerCuatrimestre" BOOLEAN NOT NULL DEFAULT TRUE,
    "AplicarReglaAnioBisiesto" BOOLEAN NOT NULL DEFAULT TRUE,
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,
    "FechaActualizacion" TIMESTAMP NOT NULL DEFAULT NOW()
);

CREATE TABLE "ConfiguracionCortesPago" (
    "Id" SERIAL PRIMARY KEY,
    "ConfiguracionSistemaId" INT NOT NULL,
    "Nombre" VARCHAR(100) NOT NULL,
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,
    FOREIGN KEY ("ConfiguracionSistemaId") REFERENCES "ConfiguracionSistema"("Id")
);

CREATE TABLE "FechasCortePago" (
    "Id" SERIAL PRIMARY KEY,
    "ConfiguracionCortePagoId" INT NOT NULL,
    "Orden" INT NOT NULL CHECK ("Orden" >= 1),
    "Dia" INT NOT NULL CHECK ("Dia" BETWEEN 1 AND 31),
    "Mes" INT NOT NULL CHECK ("Mes" BETWEEN 1 AND 12),
    FOREIGN KEY ("ConfiguracionCortePagoId") REFERENCES "ConfiguracionCortesPago"("Id")
);

CREATE UNIQUE INDEX "UX_FechasCorte_Orden"
ON "FechasCortePago" ("ConfiguracionCortePagoId", "Orden");

CREATE UNIQUE INDEX "UX_FechasCorte_Fecha"
ON "FechasCortePago" ("ConfiguracionCortePagoId", "Mes", "Dia");

CREATE TABLE "CiclosPago" (
    "Id" SERIAL PRIMARY KEY,
    "Nombre" VARCHAR(100) NOT NULL,
    "FechaInicio" DATE NOT NULL,
    "FechaFin" DATE NOT NULL
);

CREATE TABLE "PlanesPago" (
    "Id" SERIAL PRIMARY KEY,
    "CicloPagoId" INT NOT NULL,
    "Nombre" VARCHAR(100) NOT NULL,
    FOREIGN KEY ("CicloPagoId") REFERENCES "CiclosPago"("Id")
);

CREATE TABLE "ReglasPago" (
    "Id" SERIAL PRIMARY KEY,
    "PlanPagoId" INT NOT NULL,
    "Fecha" DATE NOT NULL,
    "MontoMin" NUMERIC(18,2) NOT NULL,
    "MontoMax" NUMERIC(18,2) NOT NULL,
    "TipoPago" VARCHAR(50) NOT NULL CHECK ("TipoPago" IN ('Transferencia', 'Efectivo', 'Ambos')),
    FOREIGN KEY ("PlanPagoId") REFERENCES "PlanesPago"("Id"),
    CHECK ("MontoMax" >= "MontoMin")
);

CREATE TABLE "CalculoPagos" (
    "Id" SERIAL PRIMARY KEY,
    "ContratoId" INT NOT NULL,
    "CicloPagoId" INT NOT NULL,
    "FechaCorte" DATE NOT NULL,
    "DiasCalculados" INT NOT NULL CHECK ("DiasCalculados" >= 0),
    "MontoCalculado" NUMERIC(18,2) NOT NULL,
    FOREIGN KEY ("ContratoId") REFERENCES "Contratos"("Id"),
    FOREIGN KEY ("CicloPagoId") REFERENCES "CiclosPago"("Id"),
    UNIQUE ("ContratoId", "FechaCorte")
);

CREATE TABLE "MovimientosContrato" (
    "Id" SERIAL PRIMARY KEY,
    "ContratoId" INT NOT NULL,
    "CicloPagoId" INT,
    "TipoMovimiento" VARCHAR(50) NOT NULL CHECK ("TipoMovimiento" IN ('Retiro', 'Reinversion', 'Aporte', 'UnificacionEntrada', 'UnificacionSalida', 'DesunificacionEntrada', 'DesunificacionSalida')),
    "Monto" NUMERIC(18,2) NOT NULL CHECK ("Monto" > 0),
    "FechaMovimiento" TIMESTAMP NOT NULL DEFAULT NOW(),
    "UsuarioId" INT NOT NULL,
    FOREIGN KEY ("ContratoId") REFERENCES "Contratos"("Id"),
    FOREIGN KEY ("CicloPagoId") REFERENCES "CiclosPago"("Id"),
    FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios"("Id")
);

CREATE TABLE "Pagos" (
    "Id" SERIAL PRIMARY KEY,
    "CalculoPagoId" INT NOT NULL,
    "ClienteId" INT NOT NULL,
    "CicloPagoId" INT NOT NULL,
    "GananciaBruta" NUMERIC(18,2) NOT NULL,
    "ComisionAplicada" NUMERIC(18,2) NOT NULL DEFAULT 0,
    "MontoEntregado" NUMERIC(18,2) NOT NULL,
    "DiasPagados" INT,
    "FechaInicioCalculo" DATE,
    "FechaFinCalculo" DATE,
    "MetodoPago" VARCHAR(50) NOT NULL CHECK ("MetodoPago" IN ('Transferencia', 'Efectivo', 'Mixto')),
    "Estado" VARCHAR(50) NOT NULL CHECK ("Estado" IN ('Pendiente', 'Pagado', 'Error', 'Anulado')),
    "ModalidadRendimientoAlCierre" VARCHAR(50) NOT NULL CHECK ("ModalidadRendimientoAlCierre" IN ('Normal', 'InteresCompuesto')),
    "FechaPago" TIMESTAMP,
    "EjecutadoPor" INT,
    "ClienteCuentaId" INT,
    "BancoId" INT,
    FOREIGN KEY ("CalculoPagoId") REFERENCES "CalculoPagos"("Id"),
    FOREIGN KEY ("ClienteId") REFERENCES "Clientes"("Id"),
    FOREIGN KEY ("CicloPagoId") REFERENCES "CiclosPago"("Id"),
    FOREIGN KEY ("EjecutadoPor") REFERENCES "Usuarios"("Id"),
    FOREIGN KEY ("ClienteCuentaId") REFERENCES "ClienteCuentas"("Id"),
    FOREIGN KEY ("BancoId") REFERENCES "Bancos"("Id"),
    UNIQUE ("CalculoPagoId")
);

CREATE TABLE "DecisionesPago" (
    "Id" SERIAL PRIMARY KEY,
    "PagoId" INT NOT NULL,
    "TipoDecision" VARCHAR(50) NOT NULL CHECK ("TipoDecision" IN ('RetiroTotal', 'ReinversionTotal', 'ReinversionParcial', 'TrasladoAInteresCompuesto', 'SalidaDeInteresCompuesto')),
    "MetodoRetiro" VARCHAR(50) CHECK ("MetodoRetiro" IN ('Transferencia', 'Efectivo', 'Mixto')),
    "MontoRetirado" NUMERIC(18,2) NOT NULL DEFAULT 0,
    "MontoReinvertido" NUMERIC(18,2) NOT NULL DEFAULT 0,
    "MontoAInteresCompuesto" NUMERIC(18,2) NOT NULL DEFAULT 0,
    "FechaDecision" TIMESTAMP NOT NULL DEFAULT NOW(),
    "Observacion" VARCHAR(500),
    FOREIGN KEY ("PagoId") REFERENCES "Pagos"("Id")
);

CREATE TABLE "AgendaPagos" (
    "Id" SERIAL PRIMARY KEY,
    "ClienteId" INT NOT NULL,
    "CicloPagoId" INT NOT NULL,
    "FechaProgramada" TIMESTAMP NOT NULL,
    "Tipo" VARCHAR(50) CHECK ("Tipo" IN ('Oficina', 'Transferencia')),
    "Estado" VARCHAR(50) CHECK ("Estado" IN ('Programado', 'Atendido', 'NoAsistio')),
    FOREIGN KEY ("ClienteId") REFERENCES "Clientes"("Id"),
    FOREIGN KEY ("CicloPagoId") REFERENCES "CiclosPago"("Id")
);

CREATE TABLE "AsignacionesPago" (
    "Id" SERIAL PRIMARY KEY,
    "UsuarioId" INT NOT NULL,
    "CicloPagoId" INT NOT NULL,
    "FechaAsignacion" DATE NOT NULL DEFAULT CURRENT_DATE,
    "Jornada" VARCHAR(50) CHECK ("Jornada" IN ('Manana', 'Tarde')),
    "BancoId" INT,
    "TipoPago" VARCHAR(50) CHECK ("TipoPago" IN ('Transferencia', 'Efectivo', 'Ambos')),
    "Observaciones" VARCHAR(300),
    FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios"("Id"),
    FOREIGN KEY ("CicloPagoId") REFERENCES "CiclosPago"("Id"),
    FOREIGN KEY ("BancoId") REFERENCES "Bancos"("Id")
);

CREATE TABLE "AsignacionDetalle" (
    "Id" SERIAL PRIMARY KEY,
    "AsignacionId" INT NOT NULL,
    "PagoId" INT NOT NULL UNIQUE,
    FOREIGN KEY ("AsignacionId") REFERENCES "AsignacionesPago"("Id"),
    FOREIGN KEY ("PagoId") REFERENCES "Pagos"("Id")
);

INSERT INTO "ConfiguracionSistema"
(
    "DiasCuatrimestreBase",
    "DiasMesBase",
    "ComisionEmpresaPorcentaje",
    "UsarDiasExactosPrimerCuatrimestre",
    "AplicarReglaAnioBisiesto",
    "Activo"
)
VALUES (120, 30, 5, TRUE, TRUE, TRUE);
