-- =========================================
-- LIMPIEZA
-- =========================================
DROP TABLE IF EXISTS "AsignacionDetalle" CASCADE;
DROP TABLE IF EXISTS "AsignacionesPago" CASCADE;
DROP TABLE IF EXISTS "AgendaPagos" CASCADE;
DROP TABLE IF EXISTS "Pagos" CASCADE;
DROP TABLE IF EXISTS "MovimientosContrato" CASCADE;
DROP TABLE IF EXISTS "CalculoPagos" CASCADE;
DROP TABLE IF EXISTS "ConfiguracionContrato" CASCADE;
DROP TABLE IF EXISTS "ReglasPago" CASCADE;
DROP TABLE IF EXISTS "PlanesPago" CASCADE;
DROP TABLE IF EXISTS "CiclosPago" CASCADE;
DROP TABLE IF EXISTS "Contratos" CASCADE;
DROP TABLE IF EXISTS "ClienteCuentas" CASCADE;
DROP TABLE IF EXISTS "Clientes" CASCADE;
DROP TABLE IF EXISTS "Bancos" CASCADE;
DROP TABLE IF EXISTS "Usuarios" CASCADE;

-- =========================================
-- USUARIOS
-- =========================================
CREATE TABLE "Usuarios" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Nombre" VARCHAR(150) NOT NULL,
    "Email" VARCHAR(150) UNIQUE NOT NULL,
    "PasswordHash" VARCHAR(255) NOT NULL,
    "Rol" VARCHAR(50) NOT NULL CHECK ("Rol" IN ('Admin','Ejecutivo')),
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,
    "FechaCreacion" TIMESTAMP NOT NULL DEFAULT NOW()
);

-- =========================================
-- BANCOS
-- =========================================
CREATE TABLE "Bancos" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Nombre" VARCHAR(100) UNIQUE NOT NULL
);

-- =========================================
-- CLIENTES
-- =========================================
CREATE TABLE "Clientes" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
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

-- =========================================
-- CUENTAS
-- =========================================
CREATE TABLE "ClienteCuentas" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
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

-- =========================================
-- CONTRATOS
-- =========================================
CREATE TABLE "Contratos" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ClienteId" INT NOT NULL,
    "NumeroContrato" VARCHAR(50) UNIQUE NOT NULL,
    "FechaInicio" DATE NOT NULL,
    "CapitalInicial" NUMERIC(18,2) NOT NULL CHECK ("CapitalInicial" > 0),
    "PorcentajeMensual" NUMERIC(5,2) NOT NULL CHECK ("PorcentajeMensual" > 0),
    "ComisionRetiro" NUMERIC(5,2) NOT NULL DEFAULT 0 CHECK ("ComisionRetiro" >= 0),
    "Activo" BOOLEAN NOT NULL DEFAULT TRUE,

    FOREIGN KEY ("ClienteId") REFERENCES "Clientes"("Id")
);

-- =========================================
-- CONFIGURACION DINAMICA
-- =========================================
CREATE TABLE "ConfiguracionContrato" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ContratoId" INT NOT NULL,
    "FechaInicio" DATE NOT NULL,
    "FechaFin" DATE,
    "CapitalBase" NUMERIC(18,2) NOT NULL CHECK ("CapitalBase" >= 0),
    "PorcentajeMensual" NUMERIC(5,2) NOT NULL CHECK ("PorcentajeMensual" >= 0),
    "Tipo" VARCHAR(50) NOT NULL CHECK ("Tipo" IN ('Inicial','Reinversion','Aporte','Retiro','Ajuste')),

    FOREIGN KEY ("ContratoId") REFERENCES "Contratos"("Id")
);

CREATE INDEX "IX_ConfigContrato"
ON "ConfiguracionContrato" ("ContratoId","FechaInicio");

-- =========================================
-- CICLOS
-- =========================================
CREATE TABLE "CiclosPago" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "Nombre" VARCHAR(100) NOT NULL,
    "FechaInicio" DATE NOT NULL,
    "FechaFin" DATE NOT NULL,
    CHECK ("FechaFin" >= "FechaInicio")
);

-- =========================================
-- PLANES
-- =========================================
CREATE TABLE "PlanesPago" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "CicloPagoId" INT NOT NULL,
    "Nombre" VARCHAR(100) NOT NULL,

    FOREIGN KEY ("CicloPagoId") REFERENCES "CiclosPago"("Id")
);

-- =========================================
-- REGLAS
-- =========================================
CREATE TABLE "ReglasPago" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "PlanPagoId" INT NOT NULL,
    "Fecha" DATE NOT NULL,
    "MontoMin" NUMERIC(18,2) NOT NULL,
    "MontoMax" NUMERIC(18,2) NOT NULL,
    "TipoPago" VARCHAR(50) NOT NULL CHECK ("TipoPago" IN ('Transferencia','Efectivo','Ambos')),

    FOREIGN KEY ("PlanPagoId") REFERENCES "PlanesPago"("Id"),
    CHECK ("MontoMax" >= "MontoMin")
);

-- =========================================
-- CALCULO
-- =========================================
CREATE TABLE "CalculoPagos" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ContratoId" INT NOT NULL,
    "CicloPagoId" INT NOT NULL,
    "FechaCorte" DATE NOT NULL,
    "DiasCalculados" INT NOT NULL CHECK ("DiasCalculados" >= 0),
    "MontoCalculado" NUMERIC(18,2) NOT NULL,

    FOREIGN KEY ("ContratoId") REFERENCES "Contratos"("Id"),
    FOREIGN KEY ("CicloPagoId") REFERENCES "CiclosPago"("Id"),
    UNIQUE ("ContratoId","CicloPagoId")
);

-- =========================================
-- MOVIMIENTOS
-- =========================================
CREATE TABLE "MovimientosContrato" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ContratoId" INT NOT NULL,
    "CicloPagoId" INT,
    "TipoMovimiento" VARCHAR(50) NOT NULL CHECK ("TipoMovimiento" IN ('Retiro','Reinversion','Aporte')),
    "Monto" NUMERIC(18,2) NOT NULL CHECK ("Monto" > 0),
    "FechaMovimiento" TIMESTAMP NOT NULL DEFAULT NOW(),
    "UsuarioId" INT NOT NULL,

    FOREIGN KEY ("ContratoId") REFERENCES "Contratos"("Id"),
    FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios"("Id")
);

-- =========================================
-- PAGOS
-- =========================================
CREATE TABLE "Pagos" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "CalculoPagoId" INT NOT NULL,
    "ClienteId" INT NOT NULL,
    "CicloPagoId" INT NOT NULL,
    "GananciaBruta" NUMERIC(18,2) NOT NULL,
    "ComisionAplicada" NUMERIC(18,2) NOT NULL DEFAULT 0,
    "MontoEntregado" NUMERIC(18,2) NOT NULL,
    "DiasPagados" INT,
    "FechaInicioCalculo" DATE,
    "FechaFinCalculo" DATE,
    "MetodoPago" VARCHAR(50) NOT NULL CHECK ("MetodoPago" IN ('Transferencia','Efectivo')),
    "Estado" VARCHAR(50) NOT NULL CHECK ("Estado" IN ('Pendiente','Pagado','Error')),
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

-- =========================================
-- AGENDA
-- =========================================
CREATE TABLE "AgendaPagos" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "ClienteId" INT NOT NULL,
    "CicloPagoId" INT NOT NULL,
    "FechaProgramada" TIMESTAMP NOT NULL,
    "Tipo" VARCHAR(50) CHECK ("Tipo" IN ('Oficina','Transferencia')),
    "Estado" VARCHAR(50) CHECK ("Estado" IN ('Programado','Atendido','NoAsistio')),

    FOREIGN KEY ("ClienteId") REFERENCES "Clientes"("Id"),
    FOREIGN KEY ("CicloPagoId") REFERENCES "CiclosPago"("Id")
);

-- =========================================
-- ASIGNACIONES
-- =========================================
CREATE TABLE "AsignacionesPago" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "UsuarioId" INT NOT NULL,
    "CicloPagoId" INT NOT NULL,
    "FechaAsignacion" DATE NOT NULL DEFAULT CURRENT_DATE,
    "Jornada" VARCHAR(50) CHECK ("Jornada" IN ('Manana','Tarde')),
    "BancoId" INT,
    "TipoPago" VARCHAR(50) CHECK ("TipoPago" IN ('Transferencia','Efectivo')),
    "Observaciones" VARCHAR(300),

    FOREIGN KEY ("UsuarioId") REFERENCES "Usuarios"("Id"),
    FOREIGN KEY ("CicloPagoId") REFERENCES "CiclosPago"("Id"),
    FOREIGN KEY ("BancoId") REFERENCES "Bancos"("Id")
);

CREATE TABLE "AsignacionDetalle" (
    "Id" INT GENERATED ALWAYS AS IDENTITY PRIMARY KEY,
    "AsignacionId" INT NOT NULL,
    "PagoId" INT NOT NULL UNIQUE,

    FOREIGN KEY ("AsignacionId") REFERENCES "AsignacionesPago"("Id"),
    FOREIGN KEY ("PagoId") REFERENCES "Pagos"("Id")
);