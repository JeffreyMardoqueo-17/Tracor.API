using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using Tradecorp.Domain.Models.Entities;
using Tradecorp.Domain.Models.Enums;

namespace Tradecorp.Infrastructure.Data.Configurations;

/// <summary>
/// Configuración Fluent API para la entidad Contrato.
/// Define conversiones de enums, constraints, índices y relaciones.
/// </summary>
public class ContratoConfiguration : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.ToTable("Contratos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.NumeroContrato)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.CapitalInicial)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.CapitalActual)
            .HasPrecision(18, 2)
            .IsRequired();

        builder.Property(x => x.PorcentajeMensual)
            .HasField("_porcentajeMensual")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasPrecision(5, 2)
            .IsRequired();

        builder.Property(x => x.ComisionRetiro)
            .HasField("_comisionRetiro")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasPrecision(5, 2)
            .HasDefaultValue(0);

        // Convertir enum a string para PostgreSQL
        builder.Property(x => x.ModalidadRendimiento)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        // Convertir EstadoContrato a string
        builder.Property(x => x.Estado)
            .HasField("_estado")
            .UsePropertyAccessMode(PropertyAccessMode.Field)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.PermiteUnificacion)
            .HasDefaultValue(true);

        builder.Property(x => x.FechaCreacion)
            .HasDefaultValueSql("NOW()");

        builder.Ignore(x => x.Activo);
        builder.Ignore(x => x.PeriodoPago);

        // CHECK constraints
        builder.HasCheckConstraint(
            "CK_Contratos_CapitalInicial",
            "\"CapitalInicial\" > 0");

        builder.HasCheckConstraint(
            "CK_Contratos_CapitalActual",
            "\"CapitalActual\" >= 0");

        builder.HasCheckConstraint(
            "CK_Contratos_ModalidadRendimiento",
            "\"ModalidadRendimiento\" IN ('Normal', 'InteresCompuesto')");

        builder.HasCheckConstraint(
            "CK_Contratos_Estado",
            "\"Estado\" IN ('Activo', 'Finalizado', 'Unificado', 'Anulado')");

        // Índices
        builder.HasIndex(x => x.NumeroContrato).IsUnique();
        builder.HasIndex(x => x.ClienteId);
        builder.HasIndex(x => x.Estado);

        // Relaciones
        builder.HasOne(x => x.Cliente)
            .WithMany(x => x.Contratos)
            .HasForeignKey(x => x.ClienteId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// Configuración para ContratoBeneficiario.
/// </summary>
public class ContratoBeneficiarioConfiguration : IEntityTypeConfiguration<ContratoBeneficiario>
{
    public void Configure(EntityTypeBuilder<ContratoBeneficiario> builder)
    {
        builder.ToTable("ContratoBeneficiarios");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.PorcentajeAsignado)
            .HasPrecision(5, 2)
            .IsRequired();

        // CHECK constraint
        builder.HasCheckConstraint(
            "CK_ContratoBeneficiarios_Porcentaje",
            "\"PorcentajeAsignado\" >= 0 AND \"PorcentajeAsignado\" <= 100");

        // Relaciones
        builder.HasOne(x => x.Contrato)
            .WithMany(x => x.Beneficiarios)
            .HasForeignKey(x => x.ContratoId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(x => x.ClienteBeneficiario)
            .WithMany()
            .HasForeignKey(x => x.ClienteBeneficiarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// Configuración para ContratoRelacion.
/// </summary>
public class ContratoRelacionConfiguration : IEntityTypeConfiguration<ContratoRelacion>
{
    public void Configure(EntityTypeBuilder<ContratoRelacion> builder)
    {
        builder.ToTable("ContratoRelaciones");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TipoRelacion)
            .HasConversion<string>()
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(x => x.MontoTransferido)
            .HasPrecision(18, 2);

        builder.Property(x => x.GrupoOperacionId)
            .HasDefaultValueSql("gen_random_uuid()");

        builder.Property(x => x.FechaRelacion)
            .HasDefaultValueSql("NOW()");

        // Índices
        builder.HasIndex(x => new { x.ContratoOrigenId, x.ContratoDestinoId, x.FechaRelacion });

        // Relaciones
        builder.HasOne(x => x.ContratoOrigen)
            .WithMany(x => x.RelacionesComoOrigen)
            .HasForeignKey(x => x.ContratoOrigenId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.ContratoDestino)
            .WithMany(x => x.RelacionesComoDestino)
            .HasForeignKey(x => x.ContratoDestinoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Usuario)
            .WithMany(x => x.RelacionesContratoEjecutadas)
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}

/// <summary>
/// Configuración para AuditoriaContrato.
/// </summary>
public class AuditoriaContratoConfiguration : IEntityTypeConfiguration<AuditoriaContrato>
{
    public void Configure(EntityTypeBuilder<AuditoriaContrato> builder)
    {
        builder.ToTable("AuditoriaContratos");
        builder.HasKey(x => x.Id);

        builder.Property(x => x.TipoMovimiento)
            .HasConversion<string>()
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(x => x.ValorAnterior)
            .HasColumnType("jsonb");

        builder.Property(x => x.ValorNuevo)
            .HasColumnType("jsonb");

        builder.Property(x => x.Observacion)
            .HasMaxLength(1000);

        builder.Property(x => x.FechaMovimiento)
            .HasDefaultValueSql("NOW()");

        // Índices
        builder.HasIndex(x => x.ContratoId);

        // Relaciones
        builder.HasOne(x => x.Contrato)
            .WithMany()
            .HasForeignKey(x => x.ContratoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(x => x.Usuario)
            .WithMany()
            .HasForeignKey(x => x.UsuarioId)
            .OnDelete(DeleteBehavior.Restrict);
    }
}
