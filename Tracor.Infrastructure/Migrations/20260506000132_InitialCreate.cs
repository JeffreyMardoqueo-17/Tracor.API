using System;
using Microsoft.EntityFrameworkCore.Migrations;
using Npgsql.EntityFrameworkCore.PostgreSQL.Metadata;

#nullable disable

namespace Tradecorp.Infrastructure.Migrations
{
    /// <inheritdoc />
    public partial class InitialCreate : Migration
    {
        /// <inheritdoc />
        protected override void Up(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.CreateTable(
                name: "Bancos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Bancos", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "CiclosPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CiclosPago", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionSistema",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    DiasCuatrimestreBase = table.Column<int>(type: "integer", nullable: false, defaultValue: 120),
                    DiasMesBase = table.Column<int>(type: "integer", nullable: false, defaultValue: 30),
                    ComisionEmpresaPorcentaje = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 5m),
                    UsarDiasExactosPrimerCuatrimestre = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    AplicarReglaAnioBisiesto = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionSistema", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "Usuarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    Nombre = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    Email = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: false),
                    PasswordHash = table.Column<string>(type: "character varying(255)", maxLength: 255, nullable: false),
                    Rol = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Usuarios", x => x.Id);
                });

            migrationBuilder.CreateTable(
                name: "PlanesPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CicloPagoId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_PlanesPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_PlanesPago_CiclosPago_CicloPagoId",
                        column: x => x.CicloPagoId,
                        principalTable: "CiclosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionCortesPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConfiguracionSistemaId = table.Column<int>(type: "integer", nullable: false),
                    Nombre = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionCortesPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionCortesPago_ConfiguracionSistema_ConfiguracionS~",
                        column: x => x.ConfiguracionSistemaId,
                        principalTable: "ConfiguracionSistema",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AsignacionesPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false),
                    CicloPagoId = table.Column<int>(type: "integer", nullable: false),
                    FechaAsignacion = table.Column<DateOnly>(type: "date", nullable: false, defaultValueSql: "CURRENT_DATE"),
                    Jornada = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    BancoId = table.Column<int>(type: "integer", nullable: true),
                    TipoPago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Observaciones = table.Column<string>(type: "character varying(300)", maxLength: 300, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionesPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsignacionesPago_Bancos_BancoId",
                        column: x => x.BancoId,
                        principalTable: "Bancos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsignacionesPago_CiclosPago_CicloPagoId",
                        column: x => x.CicloPagoId,
                        principalTable: "CiclosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsignacionesPago_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Clientes",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CodigoCliente = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    TipoDocumento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    NumeroDocumento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    TipoPersona = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    UsuarioEjecutivoId = table.Column<int>(type: "integer", nullable: false),
                    Correo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Notas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Clientes", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Clientes_Usuarios_UsuarioEjecutivoId",
                        column: x => x.UsuarioEjecutivoId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ReglasPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PlanPagoId = table.Column<int>(type: "integer", nullable: false),
                    Fecha = table.Column<DateOnly>(type: "date", nullable: false),
                    MontoMin = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    MontoMax = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    TipoPago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ReglasPago", x => x.Id);
                    table.CheckConstraint("CK_ReglasPago_MontoMax", "\"MontoMax\" >= \"MontoMin\"");
                    table.ForeignKey(
                        name: "FK_ReglasPago_PlanesPago_PlanPagoId",
                        column: x => x.PlanPagoId,
                        principalTable: "PlanesPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "FechasCortePago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ConfiguracionCortePagoId = table.Column<int>(type: "integer", nullable: false),
                    Orden = table.Column<int>(type: "integer", nullable: false),
                    Dia = table.Column<int>(type: "integer", nullable: false),
                    Mes = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_FechasCortePago", x => x.Id);
                    table.CheckConstraint("CK_FechasCortePago_Dia", "\"Dia\" BETWEEN 1 AND 31");
                    table.ForeignKey(
                        name: "FK_FechasCortePago_ConfiguracionCortesPago_ConfiguracionCorteP~",
                        column: x => x.ConfiguracionCortePagoId,
                        principalTable: "ConfiguracionCortesPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AgendaPagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    CicloPagoId = table.Column<int>(type: "integer", nullable: false),
                    FechaProgramada = table.Column<DateTime>(type: "timestamp with time zone", nullable: false),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AgendaPagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AgendaPagos_CiclosPago_CicloPagoId",
                        column: x => x.CicloPagoId,
                        principalTable: "CiclosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AgendaPagos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClienteCuentas",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    BancoId = table.Column<int>(type: "integer", nullable: false),
                    NumeroCuenta = table.Column<string>(type: "character varying(100)", maxLength: 100, nullable: false),
                    TipoCuenta = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    EsPrincipal = table.Column<bool>(type: "boolean", nullable: false, defaultValue: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClienteCuentas", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClienteCuentas_Bancos_BancoId",
                        column: x => x.BancoId,
                        principalTable: "Bancos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClienteCuentas_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClientesBeneficiarios",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DUI = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Correo = table.Column<string>(type: "character varying(150)", maxLength: 150, nullable: true),
                    Telefono = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    Direccion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    Porcentaje = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TipoRelacion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Notas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    FechaActualizacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()")
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientesBeneficiarios", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientesBeneficiarios_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Cascade);
                });

            migrationBuilder.CreateTable(
                name: "Contratos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    NumeroContrato = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    CapitalInicial = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PorcentajeMensual = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    ComisionRetiro = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false, defaultValue: 0m),
                    ModalidadRendimiento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    PermiteUnificacion = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    Activo = table.Column<bool>(type: "boolean", nullable: false, defaultValue: true),
                    FechaCreacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    FechaCierre = table.Column<DateTime>(type: "timestamp with time zone", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Contratos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Contratos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ClientesBeneficiariosHistorico",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ClienteBeneficiarioId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    NombreCompleto = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    DUI = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    PorcentajeAsignado = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    TipoRelacion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Evento = table.Column<string>(type: "character varying(200)", maxLength: 200, nullable: false),
                    FechaEvento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Notas = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ClientesBeneficiariosHistorico", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ClientesBeneficiariosHistorico_ClientesBeneficiarios_Client~",
                        column: x => x.ClienteBeneficiarioId,
                        principalTable: "ClientesBeneficiarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ClientesBeneficiariosHistorico_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "CalculoPagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContratoId = table.Column<int>(type: "integer", nullable: false),
                    CicloPagoId = table.Column<int>(type: "integer", nullable: false),
                    FechaCorte = table.Column<DateOnly>(type: "date", nullable: false),
                    DiasCalculados = table.Column<int>(type: "integer", nullable: false),
                    MontoCalculado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_CalculoPagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_CalculoPagos_CiclosPago_CicloPagoId",
                        column: x => x.CicloPagoId,
                        principalTable: "CiclosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_CalculoPagos_Contratos_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ConfiguracionContrato",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContratoId = table.Column<int>(type: "integer", nullable: false),
                    FechaInicio = table.Column<DateOnly>(type: "date", nullable: false),
                    FechaFin = table.Column<DateOnly>(type: "date", nullable: true),
                    CapitalBase = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    PorcentajeMensual = table.Column<decimal>(type: "numeric(5,2)", precision: 5, scale: 2, nullable: false),
                    Tipo = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ConfiguracionContrato", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ConfiguracionContrato_Contratos_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "ContratoRelaciones",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContratoOrigenId = table.Column<int>(type: "integer", nullable: false),
                    ContratoDestinoId = table.Column<int>(type: "integer", nullable: false),
                    TipoRelacion = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FechaRelacion = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    MontoTransferido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: true),
                    Observacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true),
                    GrupoOperacionId = table.Column<Guid>(type: "uuid", nullable: false, defaultValueSql: "gen_random_uuid()"),
                    UsuarioId = table.Column<int>(type: "integer", nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_ContratoRelaciones", x => x.Id);
                    table.ForeignKey(
                        name: "FK_ContratoRelaciones_Contratos_ContratoDestinoId",
                        column: x => x.ContratoDestinoId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContratoRelaciones_Contratos_ContratoOrigenId",
                        column: x => x.ContratoOrigenId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_ContratoRelaciones_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "MovimientosContrato",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    ContratoId = table.Column<int>(type: "integer", nullable: false),
                    CicloPagoId = table.Column<int>(type: "integer", nullable: true),
                    TipoMovimiento = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Monto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    FechaMovimiento = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    UsuarioId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_MovimientosContrato", x => x.Id);
                    table.ForeignKey(
                        name: "FK_MovimientosContrato_CiclosPago_CicloPagoId",
                        column: x => x.CicloPagoId,
                        principalTable: "CiclosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosContrato_Contratos_ContratoId",
                        column: x => x.ContratoId,
                        principalTable: "Contratos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_MovimientosContrato_Usuarios_UsuarioId",
                        column: x => x.UsuarioId,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "Pagos",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    CalculoPagoId = table.Column<int>(type: "integer", nullable: false),
                    ClienteId = table.Column<int>(type: "integer", nullable: false),
                    CicloPagoId = table.Column<int>(type: "integer", nullable: false),
                    GananciaBruta = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    ComisionAplicada = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    MontoEntregado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false),
                    DiasPagados = table.Column<int>(type: "integer", nullable: true),
                    FechaInicioCalculo = table.Column<DateOnly>(type: "date", nullable: true),
                    FechaFinCalculo = table.Column<DateOnly>(type: "date", nullable: true),
                    MetodoPago = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    Estado = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    FechaPago = table.Column<DateTime>(type: "timestamp with time zone", nullable: true),
                    EjecutadoPor = table.Column<int>(type: "integer", nullable: true),
                    ClienteCuentaId = table.Column<int>(type: "integer", nullable: true),
                    BancoId = table.Column<int>(type: "integer", nullable: true),
                    ModalidadRendimientoAlCierre = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_Pagos", x => x.Id);
                    table.ForeignKey(
                        name: "FK_Pagos_Bancos_BancoId",
                        column: x => x.BancoId,
                        principalTable: "Bancos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagos_CalculoPagos_CalculoPagoId",
                        column: x => x.CalculoPagoId,
                        principalTable: "CalculoPagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagos_CiclosPago_CicloPagoId",
                        column: x => x.CicloPagoId,
                        principalTable: "CiclosPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagos_ClienteCuentas_ClienteCuentaId",
                        column: x => x.ClienteCuentaId,
                        principalTable: "ClienteCuentas",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagos_Clientes_ClienteId",
                        column: x => x.ClienteId,
                        principalTable: "Clientes",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_Pagos_Usuarios_EjecutadoPor",
                        column: x => x.EjecutadoPor,
                        principalTable: "Usuarios",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "AsignacionDetalle",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    AsignacionId = table.Column<int>(type: "integer", nullable: false),
                    PagoId = table.Column<int>(type: "integer", nullable: false)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_AsignacionDetalle", x => x.Id);
                    table.ForeignKey(
                        name: "FK_AsignacionDetalle_AsignacionesPago_AsignacionId",
                        column: x => x.AsignacionId,
                        principalTable: "AsignacionesPago",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                    table.ForeignKey(
                        name: "FK_AsignacionDetalle_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateTable(
                name: "DecisionesPago",
                columns: table => new
                {
                    Id = table.Column<int>(type: "integer", nullable: false)
                        .Annotation("Npgsql:ValueGenerationStrategy", NpgsqlValueGenerationStrategy.IdentityByDefaultColumn),
                    PagoId = table.Column<int>(type: "integer", nullable: false),
                    TipoDecision = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: false),
                    MetodoRetiro = table.Column<string>(type: "character varying(50)", maxLength: 50, nullable: true),
                    MontoRetirado = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    MontoReinvertido = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    MontoAInteresCompuesto = table.Column<decimal>(type: "numeric(18,2)", precision: 18, scale: 2, nullable: false, defaultValue: 0m),
                    FechaDecision = table.Column<DateTime>(type: "timestamp with time zone", nullable: false, defaultValueSql: "NOW()"),
                    Observacion = table.Column<string>(type: "character varying(500)", maxLength: 500, nullable: true)
                },
                constraints: table =>
                {
                    table.PrimaryKey("PK_DecisionesPago", x => x.Id);
                    table.ForeignKey(
                        name: "FK_DecisionesPago_Pagos_PagoId",
                        column: x => x.PagoId,
                        principalTable: "Pagos",
                        principalColumn: "Id",
                        onDelete: ReferentialAction.Restrict);
                });

            migrationBuilder.CreateIndex(
                name: "IX_AgendaPagos_CicloPagoId",
                table: "AgendaPagos",
                column: "CicloPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_AgendaPagos_ClienteId",
                table: "AgendaPagos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionDetalle_AsignacionId",
                table: "AsignacionDetalle",
                column: "AsignacionId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionDetalle_PagoId",
                table: "AsignacionDetalle",
                column: "PagoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesPago_BancoId",
                table: "AsignacionesPago",
                column: "BancoId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesPago_CicloPagoId",
                table: "AsignacionesPago",
                column: "CicloPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_AsignacionesPago_UsuarioId",
                table: "AsignacionesPago",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Bancos_Nombre",
                table: "Bancos",
                column: "Nombre",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_CalculoPagos_CicloPagoId",
                table: "CalculoPagos",
                column: "CicloPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_CalculoPagos_ContratoId_FechaCorte",
                table: "CalculoPagos",
                columns: new[] { "ContratoId", "FechaCorte" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_ClienteCuentas_BancoId",
                table: "ClienteCuentas",
                column: "BancoId");

            migrationBuilder.CreateIndex(
                name: "IX_ClienteCuentas_ClienteId",
                table: "ClienteCuentas",
                column: "ClienteId",
                unique: true,
                filter: "\"EsPrincipal\" = TRUE");

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_CodigoCliente",
                table: "Clientes",
                column: "CodigoCliente",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_NumeroDocumento",
                table: "Clientes",
                column: "NumeroDocumento",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Clientes_UsuarioEjecutivoId",
                table: "Clientes",
                column: "UsuarioEjecutivoId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesBeneficiarios_ClienteId",
                table: "ClientesBeneficiarios",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesBeneficiarios_ClienteId_Estado",
                table: "ClientesBeneficiarios",
                columns: new[] { "ClienteId", "Estado" },
                filter: "\"Estado\" = 'Activo'");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesBeneficiariosHistorico_ClienteBeneficiarioId",
                table: "ClientesBeneficiariosHistorico",
                column: "ClienteBeneficiarioId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesBeneficiariosHistorico_ClienteId",
                table: "ClientesBeneficiariosHistorico",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_ClientesBeneficiariosHistorico_FechaEvento",
                table: "ClientesBeneficiariosHistorico",
                column: "FechaEvento");

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionContrato_ContratoId_FechaInicio",
                table: "ConfiguracionContrato",
                columns: new[] { "ContratoId", "FechaInicio" });

            migrationBuilder.CreateIndex(
                name: "IX_ConfiguracionCortesPago_ConfiguracionSistemaId",
                table: "ConfiguracionCortesPago",
                column: "ConfiguracionSistemaId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratoRelaciones_ContratoDestinoId",
                table: "ContratoRelaciones",
                column: "ContratoDestinoId");

            migrationBuilder.CreateIndex(
                name: "IX_ContratoRelaciones_ContratoOrigenId_ContratoDestinoId_Fecha~",
                table: "ContratoRelaciones",
                columns: new[] { "ContratoOrigenId", "ContratoDestinoId", "FechaRelacion" });

            migrationBuilder.CreateIndex(
                name: "IX_ContratoRelaciones_UsuarioId",
                table: "ContratoRelaciones",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_ClienteId",
                table: "Contratos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Contratos_NumeroContrato",
                table: "Contratos",
                column: "NumeroContrato",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_DecisionesPago_PagoId",
                table: "DecisionesPago",
                column: "PagoId");

            migrationBuilder.CreateIndex(
                name: "IX_FechasCortePago_ConfiguracionCortePagoId_Mes_Dia",
                table: "FechasCortePago",
                columns: new[] { "ConfiguracionCortePagoId", "Mes", "Dia" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_FechasCortePago_ConfiguracionCortePagoId_Orden",
                table: "FechasCortePago",
                columns: new[] { "ConfiguracionCortePagoId", "Orden" },
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosContrato_CicloPagoId",
                table: "MovimientosContrato",
                column: "CicloPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosContrato_ContratoId",
                table: "MovimientosContrato",
                column: "ContratoId");

            migrationBuilder.CreateIndex(
                name: "IX_MovimientosContrato_UsuarioId",
                table: "MovimientosContrato",
                column: "UsuarioId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_BancoId",
                table: "Pagos",
                column: "BancoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_CalculoPagoId",
                table: "Pagos",
                column: "CalculoPagoId",
                unique: true);

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_CicloPagoId",
                table: "Pagos",
                column: "CicloPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ClienteCuentaId",
                table: "Pagos",
                column: "ClienteCuentaId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_ClienteId",
                table: "Pagos",
                column: "ClienteId");

            migrationBuilder.CreateIndex(
                name: "IX_Pagos_EjecutadoPor",
                table: "Pagos",
                column: "EjecutadoPor");

            migrationBuilder.CreateIndex(
                name: "IX_PlanesPago_CicloPagoId",
                table: "PlanesPago",
                column: "CicloPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_ReglasPago_PlanPagoId",
                table: "ReglasPago",
                column: "PlanPagoId");

            migrationBuilder.CreateIndex(
                name: "IX_Usuarios_Email",
                table: "Usuarios",
                column: "Email",
                unique: true);
        }

        /// <inheritdoc />
        protected override void Down(MigrationBuilder migrationBuilder)
        {
            migrationBuilder.DropTable(
                name: "AgendaPagos");

            migrationBuilder.DropTable(
                name: "AsignacionDetalle");

            migrationBuilder.DropTable(
                name: "ClientesBeneficiariosHistorico");

            migrationBuilder.DropTable(
                name: "ConfiguracionContrato");

            migrationBuilder.DropTable(
                name: "ContratoRelaciones");

            migrationBuilder.DropTable(
                name: "DecisionesPago");

            migrationBuilder.DropTable(
                name: "FechasCortePago");

            migrationBuilder.DropTable(
                name: "MovimientosContrato");

            migrationBuilder.DropTable(
                name: "ReglasPago");

            migrationBuilder.DropTable(
                name: "AsignacionesPago");

            migrationBuilder.DropTable(
                name: "ClientesBeneficiarios");

            migrationBuilder.DropTable(
                name: "Pagos");

            migrationBuilder.DropTable(
                name: "ConfiguracionCortesPago");

            migrationBuilder.DropTable(
                name: "PlanesPago");

            migrationBuilder.DropTable(
                name: "CalculoPagos");

            migrationBuilder.DropTable(
                name: "ClienteCuentas");

            migrationBuilder.DropTable(
                name: "ConfiguracionSistema");

            migrationBuilder.DropTable(
                name: "CiclosPago");

            migrationBuilder.DropTable(
                name: "Contratos");

            migrationBuilder.DropTable(
                name: "Bancos");

            migrationBuilder.DropTable(
                name: "Clientes");

            migrationBuilder.DropTable(
                name: "Usuarios");
        }
    }
}
