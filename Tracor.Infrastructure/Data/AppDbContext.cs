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
    public DbSet<Contrato> Contratos => Set<Contrato>();
    public DbSet<ConfiguracionContrato> ConfiguracionesContrato => Set<ConfiguracionContrato>();
    public DbSet<CicloPago> CiclosPago => Set<CicloPago>();
    public DbSet<PlanPago> PlanesPago => Set<PlanPago>();
    public DbSet<ReglaPago> ReglasPago => Set<ReglaPago>();
    public DbSet<CalculoPago> CalculosPago => Set<CalculoPago>();
    public DbSet<MovimientoContrato> MovimientosContrato => Set<MovimientoContrato>();
    public DbSet<Pago> Pagos => Set<Pago>();
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
        ConfigureContratos(modelBuilder);
        ConfigureConfiguracionContrato(modelBuilder);
        ConfigureCiclosPago(modelBuilder);
        ConfigurePlanesPago(modelBuilder);
        ConfigureReglasPago(modelBuilder);
        ConfigureCalculosPago(modelBuilder);
        ConfigureMovimientosContrato(modelBuilder);
        ConfigurePagos(modelBuilder);
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
        entity.Property(x => x.Rol).HasConversion<string>().HasMaxLength(50).IsRequired();
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
        entity.Property(x => x.Activo).HasDefaultValue(true);
        entity.HasIndex(x => x.NumeroContrato).IsUnique();

        entity.HasOne(x => x.Cliente)
            .WithMany(x => x.Contratos)
            .HasForeignKey(x => x.ClienteId)
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
        entity.HasIndex(x => new { x.ContratoId, x.CicloPagoId }).IsUnique();

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
}