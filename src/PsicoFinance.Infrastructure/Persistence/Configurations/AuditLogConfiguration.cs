using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Infrastructure.Persistence.Configurations;

public class AuditLogConfiguration : IEntityTypeConfiguration<AuditLog>
{
    public void Configure(EntityTypeBuilder<AuditLog> builder)
    {
        builder.HasKey(a => a.Id);

        builder.Property(a => a.Acao)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(a => a.Entidade)
            .HasMaxLength(100)
            .IsRequired();

        builder.Property(a => a.DadosAnteriores)
            .HasColumnType("jsonb");

        builder.Property(a => a.DadosNovos)
            .HasColumnType("jsonb");

        builder.Property(a => a.IpOrigem)
            .HasMaxLength(45);

        builder.HasIndex(a => a.ClinicaId);
        builder.HasIndex(a => new { a.Entidade, a.EntidadeId });
        builder.HasIndex(a => a.CriadoEm);
    }
}
