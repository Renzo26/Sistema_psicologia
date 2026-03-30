using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Infrastructure.Persistence.Configurations;

public class SessaoConfiguration : IEntityTypeConfiguration<Sessao>
{
    public void Configure(EntityTypeBuilder<Sessao> builder)
    {
        builder.HasKey(s => s.Id);
        builder.Property(s => s.Status).HasConversion<string>().HasMaxLength(20);
        builder.Property(s => s.MotivoFalta).HasMaxLength(500);
        builder.Property(s => s.Observacoes).HasMaxLength(2000);
        builder.Property(s => s.DuracaoMinutos).HasDefaultValue(50);

        builder.HasOne(s => s.Clinica)
               .WithMany(c => c.Sessoes)
               .HasForeignKey(s => s.ClinicaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(s => s.Contrato)
               .WithMany()
               .HasForeignKey(s => s.ContratoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Paciente)
               .WithMany()
               .HasForeignKey(s => s.PacienteId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(s => s.Psicologo)
               .WithMany()
               .HasForeignKey(s => s.PsicologoId)
               .OnDelete(DeleteBehavior.Restrict);

        builder.HasIndex(s => new { s.ContratoId, s.Data });
        builder.HasIndex(s => s.PacienteId);
        builder.HasIndex(s => s.PsicologoId);
        builder.HasIndex(s => s.Data);
        builder.HasIndex(s => s.Status);
    }
}
