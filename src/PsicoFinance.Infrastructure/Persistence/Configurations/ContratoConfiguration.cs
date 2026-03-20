using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Infrastructure.Persistence.Configurations;

public class ContratoConfiguration : IEntityTypeConfiguration<Contrato>
{
    public void Configure(EntityTypeBuilder<Contrato> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.ValorSessao).HasPrecision(10, 2).IsRequired();
        builder.Property(c => c.FormaPagamento).HasConversion<string>().HasMaxLength(20);
        builder.Property(c => c.Frequencia).HasConversion<string>().HasMaxLength(15);
        builder.Property(c => c.DiaSemanasessao).HasConversion<string>().HasMaxLength(10);
        builder.Property(c => c.DuracaoMinutos).HasDefaultValue(50);
        builder.Property(c => c.Status).HasConversion<string>().HasMaxLength(15);
        builder.Property(c => c.MotivoEncerramento).HasMaxLength(300);

        builder.HasOne(c => c.Clinica)
               .WithMany(cl => cl.Contratos)
               .HasForeignKey(c => c.ClinicaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(c => c.Paciente)
               .WithMany(p => p.Contratos)
               .HasForeignKey(c => c.PacienteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.Psicologo)
               .WithMany(p => p.Contratos)
               .HasForeignKey(c => c.PsicologoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(c => c.PlanoConta)
               .WithMany(p => p.Contratos)
               .HasForeignKey(c => c.PlanoContaId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(c => c.PacienteId);
        builder.HasIndex(c => c.PsicologoId);
        builder.HasIndex(c => c.Status);
    }
}
