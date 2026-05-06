using Microsoft.EntityFrameworkCore;
using Tradecorp.Domain.Models.Entities;

namespace Tradecorp.Infrastructure.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options)
        : base(options)
    {
    }

    public DbSet<Usuario> Usuarios => Set<Usuario>();
    public DbSet<Banco> Bancos => Set<Banco>();
    public DbSet<Cliente> Clientes => Set<Cliente>();
    public DbSet<ClienteCuenta> ClienteCuentas => Set<ClienteCuenta>();
    public DbSet<ClienteBeneficiario> ClientesBeneficiarios => Set<ClienteBeneficiario>();
    public DbSet<ClienteBeneficiarioHistorico> ClientesBeneficiariosHistorico => Set<ClienteBeneficiarioHistorico>();
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<ContratoRelacion> ContratoRelaciones => Set<ContratoRelacion>();
    public DbSet<ConfiguracionContrato> ConfiguracionesContrato => Set<ConfiguracionContrato>();
    public DbSet<ConfiguracionSistema> ConfiguracionesSistema => Set<ConfiguracionSistema>();
    public DbSet<ConfiguracionCortePago> ConfiguracionesCortePago => Set<ConfiguracionCortePago>();
    public DbSet<FechaCortePago> FechasCortePago => Set<FechaCortePago>();
    public DbSet<CicloPago> CiclosPago => Set<CicloPago>();
    public DbSet<PlanPago> PlanesPago => Set<PlanPago>();
    public DbSet<ReglaPago> ReglasPago => Set<ReglaPago>();
    public DbSet<CalculoPago> CalculosPago => Set<CalculoPago>();
    public DbSet<MovimientoContrato> MovimientosContrato => Set<MovimientoContrato>();
    public DbSet<Pago> Pagos => Set<Pago>();
    public DbSet<DecisionPago> DecisionesPago => Set<DecisionPago>();
    public DbSet<AgendaPago> AgendaPagos => Set<AgendaPago>();
    public DbSet<AsignacionPago> AsignacionesPago => Set<AsignacionPago>();
    public DbSet<AsignacionDetalle> AsignacionDetalle => Set<AsignacionDetalle>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        ConfigureUsuarios(modelBuilder);
        ConfigureBancos(modelBuilder);
        ConfigureClientes(modelBuilder);
        ConfigureClienteCuentas(modelBuilder);
        ConfigureClienteBeneficiarios(modelBuilder);
        ConfigureClienteBeneficiariosHistorico(modelBuilder);
        ConfigureContratos(modelBuilder);
        ConfigureContratoRelaciones(modelBuilder);
        ConfigureConfiguracionContrato(modelBuilder);
        ConfigureConfiguracionSistema(modelBuilder);
        ConfigureConfiguracionCortePago(modelBuilder);
        ConfigureFechasCortePago(modelBuilder);
        ConfigureCiclosPago(modelBuilder);
        ConfigurePlanesPago(modelBuilder);
        ConfigureReglasPago(modelBuilder);
        ConfigureCalculosPago(modelBuilder);
        ConfigureMovimientosContrato(modelBuilder);
        ConfigurePagos(modelBuilder);
        ConfigureDecisionesPago(modelBuilder);
        ConfigureAgendaPagos(modelBuilder);
        ConfigureAsignacionesPago(modelBuilder);
        ConfigureAsignacionDetalle(modelBuilder);
    }

    private static void ConfigureUsuarios(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Usuario>();

        entity.ToTable("Usuarios");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Nombre).HasMaxLength(150).IsRequired();
        entity.Property(x => x.Email).HasMaxLength(150).IsRequired();
        entity.Property(x => x.PasswordHash).HasMaxLength(255).IsRequired();

        // Almacenamos como string para compatibilidad con la base de datos existente
        entity.Property(x => x.Rol)
                .HasConversion<string>()
                .HasMaxLength(50)
                .IsRequired();

        entity.Property(x => x.Activo).HasDefaultValue(true);
        entity.Property(x => x.FechaCreacion).HasDefaultValueSql("NOW()");
        entity.HasIndex(x => x.Email).IsUnique();

    }

    private static void ConfigureBancos(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Banco>();

        entity.ToTable("Bancos");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        entity.HasIndex(x => x.Nombre).IsUnique();
    }

    private static void ConfigureClientes(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Cliente>();

        entity.ToTable("Clientes");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.CodigoCliente).HasMaxLength(50).IsRequired();
        entity.Property(x => x.NombreCompleto).HasMaxLength(200).IsRequired();
        entity.Property(x => x.TipoDocumento).HasMaxLength(50);
        entity.Property(x => x.NumeroDocumento).HasMaxLength(50);
        entity.Property(x => x.TipoPersona).HasMaxLength(50);
        entity.Property(x => x.Correo).HasMaxLength(150);
        entity.Property(x => x.Telefono).HasMaxLength(50);
        entity.Property(x => x.Notas).HasMaxLength(500);
        entity.Property(x => x.Activo).HasDefaultValue(true);

        entity.HasIndex(x => x.CodigoCliente).IsUnique();
        entity.HasIndex(x => x.NumeroDocumento).IsUnique();

        entity.HasOne(x => x.UsuarioEjecutivo)
            .WithMany(x => x.ClientesAsignados)
            .HasForeignKey(x => x.UsuarioEjecutivoId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureClienteCuentas(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ClienteCuenta>();

        entity.ToTable("ClienteCuentas");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.NumeroCuenta).HasMaxLength(100).IsRequired();
        entity.Property(x => x.TipoCuenta).HasMaxLength(50);
        entity.Property(x => x.EsPrincipal).HasDefaultValue(false);

        entity.HasOne(x => x.Cliente)
            .WithMany(x => x.ClienteCuentas)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Banco)
            .WithMany(x => x.ClienteCuentas)
            .HasForeignKey(x => x.BancoId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasIndex(x => x.ClienteId)
            .IsUnique()
            .HasFilter("\"EsPrincipal\" = TRUE");
    }

    private static void ConfigureContratos(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Contrato>();

        entity.ToTable("Contratos");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.NumeroContrato).HasMaxLength(50).IsRequired();
        entity.Property(x => x.CapitalInicial).HasPrecision(18, 2);
        entity.Property(x => x.PorcentajeMensual).HasPrecision(5, 2);
        entity.Property(x => x.ComisionRetiro).HasPrecision(5, 2).HasDefaultValue(0m);
        entity.Property(x => x.ModalidadRendimiento).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(x => x.PermiteUnificacion).HasDefaultValue(true);
        entity.Property(x => x.Activo).HasDefaultValue(true);
        entity.Property(x => x.FechaCreacion).HasDefaultValueSql("NOW()");
        entity.HasIndex(x => x.NumeroContrato).IsUnique();

        entity.HasOne(x => x.Cliente)
            .WithMany(x => x.Contratos)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureContratoRelaciones(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ContratoRelacion>();

        entity.ToTable("ContratoRelaciones");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.TipoRelacion).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(x => x.MontoTransferido).HasPrecision(18, 2);
        entity.Property(x => x.Observacion).HasMaxLength(500);
        entity.Property(x => x.FechaRelacion).HasDefaultValueSql("NOW()");
        entity.Property(x => x.GrupoOperacionId).HasDefaultValueSql("gen_random_uuid()");
        entity.HasIndex(x => new { x.ContratoOrigenId, x.ContratoDestinoId, x.FechaRelacion });

        entity.HasOne(x => x.ContratoOrigen)
            .WithMany(x => x.RelacionesComoOrigen)
            .HasForeignKey(x => x.ContratoOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.ContratoDestino)
            .WithMany(x => x.RelacionesComoDestino)
            .HasForeignKey(x => x.ContratoDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Usuario)
            .WithMany(x => x.RelacionesContratoEjecutadas)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureConfiguracionContrato(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ConfiguracionContrato>();

        entity.ToTable("ConfiguracionContrato");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.CapitalBase).HasPrecision(18, 2);
        entity.Property(x => x.PorcentajeMensual).HasPrecision(5, 2);
        entity.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.HasIndex(x => new { x.ContratoId, x.FechaInicio });

        entity.HasOne(x => x.Contrato)
            .WithMany(x => x.ConfiguracionesContrato)
            .HasForeignKey(x => x.ContratoId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureConfiguracionSistema(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ConfiguracionSistema>();

        entity.ToTable("ConfiguracionSistema");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.DiasCuatrimestreBase).HasDefaultValue(120);
        entity.Property(x => x.DiasMesBase).HasDefaultValue(30);
        entity.Property(x => x.ComisionEmpresaPorcentaje).HasPrecision(5, 2).HasDefaultValue(5m);
        entity.Property(x => x.UsarDiasExactosPrimerCuatrimestre).HasDefaultValue(true);
        entity.Property(x => x.AplicarReglaAnioBisiesto).HasDefaultValue(true);
        entity.Property(x => x.Activo).HasDefaultValue(true);
        entity.Property(x => x.FechaActualizacion).HasDefaultValueSql("NOW()");
    }

    private static void ConfigureConfiguracionCortePago(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ConfiguracionCortePago>();

        entity.ToTable("ConfiguracionCortesPago");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
        entity.Property(x => x.Activo).HasDefaultValue(true);

        entity.HasOne(x => x.ConfiguracionSistema)
            .WithMany(x => x.CortesPago)
            .HasForeignKey(x => x.ConfiguracionSistemaId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureFechasCortePago(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<FechaCortePago>();

        entity.ToTable("FechasCortePago", tableBuilder =>
            tableBuilder.HasCheckConstraint("CK_FechasCortePago_Dia", "\"Dia\" BETWEEN 1 AND 31"));
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => new { x.ConfiguracionCortePagoId, x.Orden }).IsUnique();
        entity.HasIndex(x => new { x.ConfiguracionCortePagoId, x.Mes, x.Dia }).IsUnique();

        entity.HasOne(x => x.ConfiguracionCortePago)
            .WithMany(x => x.FechasCorte)
            .HasForeignKey(x => x.ConfiguracionCortePagoId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureCiclosPago(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<CicloPago>();

        entity.ToTable("CiclosPago");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Nombre).HasMaxLength(100).IsRequired();
    }

    private static void ConfigurePlanesPago(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<PlanPago>();

        entity.ToTable("PlanesPago");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Nombre).HasMaxLength(100).IsRequired();

        entity.HasOne(x => x.CicloPago)
            .WithMany(x => x.PlanesPago)
            .HasForeignKey(x => x.CicloPagoId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureReglasPago(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ReglaPago>();

        entity.ToTable("ReglasPago", tableBuilder =>
            tableBuilder.HasCheckConstraint("CK_ReglasPago_MontoMax", "\"MontoMax\" >= \"MontoMin\""));
        entity.HasKey(x => x.Id);
        entity.Property(x => x.MontoMin).HasPrecision(18, 2);
        entity.Property(x => x.MontoMax).HasPrecision(18, 2);
        entity.Property(x => x.TipoPago).HasConversion<string>().HasMaxLength(50).IsRequired();

        entity.HasOne(x => x.PlanPago)
            .WithMany(x => x.ReglasPago)
            .HasForeignKey(x => x.PlanPagoId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureCalculosPago(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<CalculoPago>();

        entity.ToTable("CalculoPagos");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.MontoCalculado).HasPrecision(18, 2);
        entity.HasIndex(x => new { x.ContratoId, x.FechaCorte }).IsUnique();

        entity.HasOne(x => x.Contrato)
            .WithMany(x => x.CalculosPago)
            .HasForeignKey(x => x.ContratoId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.CicloPago)
            .WithMany(x => x.CalculosPago)
            .HasForeignKey(x => x.CicloPagoId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureMovimientosContrato(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<MovimientoContrato>();

        entity.ToTable("MovimientosContrato");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.TipoMovimiento).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(x => x.Monto).HasPrecision(18, 2);
        entity.Property(x => x.FechaMovimiento).HasDefaultValueSql("NOW()");

        entity.HasOne(x => x.Contrato)
            .WithMany(x => x.MovimientosContrato)
            .HasForeignKey(x => x.ContratoId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.CicloPago)
            .WithMany(x => x.MovimientosContrato)
            .HasForeignKey(x => x.CicloPagoId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Usuario)
            .WithMany(x => x.MovimientosContrato)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigurePagos(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<Pago>();

        entity.ToTable("Pagos");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.GananciaBruta).HasPrecision(18, 2);
        entity.Property(x => x.ComisionAplicada).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.MontoEntregado).HasPrecision(18, 2);
        entity.Property(x => x.MetodoPago).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(x => x.Estado).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(x => x.ModalidadRendimientoAlCierre).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.HasIndex(x => x.CalculoPagoId).IsUnique();

        entity.HasOne(x => x.CalculoPago)
            .WithOne(x => x.Pago)
            .HasForeignKey<Pago>(x => x.CalculoPagoId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Cliente)
            .WithMany(x => x.Pagos)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.CicloPago)
            .WithMany(x => x.Pagos)
            .HasForeignKey(x => x.CicloPagoId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.UsuarioEjecutadoPor)
            .WithMany(x => x.PagosEjecutados)
            .HasForeignKey(x => x.EjecutadoPor)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.ClienteCuenta)
            .WithMany(x => x.Pagos)
            .HasForeignKey(x => x.ClienteCuentaId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Banco)
            .WithMany(x => x.Pagos)
            .HasForeignKey(x => x.BancoId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureDecisionesPago(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<DecisionPago>();

        entity.ToTable("DecisionesPago");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.TipoDecision).HasConversion<string>().HasMaxLength(50).IsRequired();
        entity.Property(x => x.MetodoRetiro).HasConversion<string>().HasMaxLength(50);
        entity.Property(x => x.MontoRetirado).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.MontoReinvertido).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.MontoAInteresCompuesto).HasPrecision(18, 2).HasDefaultValue(0m);
        entity.Property(x => x.Observacion).HasMaxLength(500);
        entity.Property(x => x.FechaDecision).HasDefaultValueSql("NOW()");

        entity.HasOne(x => x.Pago)
            .WithMany(x => x.Decisiones)
            .HasForeignKey(x => x.PagoId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAgendaPagos(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AgendaPago>();

        entity.ToTable("AgendaPagos");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.Tipo).HasConversion<string>().HasMaxLength(50);
        entity.Property(x => x.Estado).HasConversion<string>().HasMaxLength(50);

        entity.HasOne(x => x.Cliente)
            .WithMany(x => x.AgendaPagos)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.CicloPago)
            .WithMany(x => x.AgendaPagos)
            .HasForeignKey(x => x.CicloPagoId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAsignacionesPago(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AsignacionPago>();

        entity.ToTable("AsignacionesPago");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.FechaAsignacion).HasDefaultValueSql("CURRENT_DATE");
        entity.Property(x => x.Jornada).HasConversion<string>().HasMaxLength(50);
        entity.Property(x => x.TipoPago).HasConversion<string>().HasMaxLength(50);
        entity.Property(x => x.Observaciones).HasMaxLength(300);

        entity.HasOne(x => x.Usuario)
            .WithMany(x => x.AsignacionesPago)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.CicloPago)
            .WithMany(x => x.AsignacionesPago)
            .HasForeignKey(x => x.CicloPagoId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Banco)
            .WithMany(x => x.AsignacionesPago)
            .HasForeignKey(x => x.BancoId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureAsignacionDetalle(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<AsignacionDetalle>();

        entity.ToTable("AsignacionDetalle");
        entity.HasKey(x => x.Id);
        entity.HasIndex(x => x.PagoId).IsUnique();

        entity.HasOne(x => x.Asignacion)
            .WithMany(x => x.Detalles)
            .HasForeignKey(x => x.AsignacionId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Pago)
            .WithOne(x => x.AsignacionDetalle)
            .HasForeignKey<AsignacionDetalle>(x => x.PagoId)
            .OnDelete(DeleteBehavior.Restrict);
    }

    private static void ConfigureClienteBeneficiarios(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ClienteBeneficiario>();

        entity.ToTable("ClientesBeneficiarios");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.NombreCompleto).HasMaxLength(200).IsRequired();
        entity.Property(x => x.DUI).HasMaxLength(50);
        entity.Property(x => x.Correo).HasMaxLength(150);
        entity.Property(x => x.Telefono).HasMaxLength(50);
        entity.Property(x => x.Direccion).HasMaxLength(500);
        entity.Property(x => x.Porcentaje).HasPrecision(5, 2).IsRequired();
        entity.Property(x => x.TipoRelacion)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        entity.Property(x => x.Estado)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        entity.Property(x => x.Notas).HasMaxLength(500);
        entity.Property(x => x.FechaCreacion).HasDefaultValueSql("NOW()");
        entity.Property(x => x.FechaActualizacion).HasDefaultValueSql("NOW()");

        entity.HasIndex(x => x.ClienteId);
        entity.HasIndex(x => new { x.ClienteId, x.Estado })
            .HasFilter("\"Estado\" = 'Activo'");

        entity.HasOne(x => x.Cliente)
            .WithMany(x => x.Beneficiarios)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Cascade);
    }

    private static void ConfigureClienteBeneficiariosHistorico(ModelBuilder modelBuilder)
    {
        var entity = modelBuilder.Entity<ClienteBeneficiarioHistorico>();

        entity.ToTable("ClientesBeneficiariosHistorico");
        entity.HasKey(x => x.Id);
        entity.Property(x => x.NombreCompleto).HasMaxLength(200).IsRequired();
        entity.Property(x => x.DUI).HasMaxLength(50);
        entity.Property(x => x.PorcentajeAsignado).HasPrecision(5, 2).IsRequired();
        entity.Property(x => x.TipoRelacion)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();
        entity.Property(x => x.Evento).HasMaxLength(200).IsRequired();
        entity.Property(x => x.FechaEvento).HasDefaultValueSql("NOW()");
        entity.Property(x => x.Notas).HasMaxLength(500);

        entity.HasIndex(x => x.ClienteId);
        entity.HasIndex(x => x.ClienteBeneficiarioId);
        entity.HasIndex(x => x.FechaEvento);

        entity.HasOne(x => x.ClienteBeneficiario)
            .WithMany(x => x.Historico)
            .HasForeignKey(x => x.ClienteBeneficiarioId)
            .OnDelete(DeleteBehavior.Restrict);

        entity.HasOne(x => x.Cliente)
            .WithMany(x => x.BeneficiariosHistorico)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
