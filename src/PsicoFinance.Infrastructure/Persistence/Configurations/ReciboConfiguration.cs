using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Infrastructure.Persistence.Configurations;

public class ReciboConfiguration : IEntityTypeConfiguration<Recibo>
{
    public void Configure(EntityTypeBuilder<Recibo> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.NumeroRecibo)
            .HasMaxLength(50)
            .IsRequired();

        builder.Property(r => r.Valor)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(r => r.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(r => r.ArquivoUrl).HasMaxLength(1000);
        builder.Property(r => r.ArquivoNome).HasMaxLength(200);

        builder.HasOne(r => r.Sessao)
            .WithMany()
            .HasForeignKey(r => r.SessaoId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Paciente)
            .WithMany()
            .HasForeignKey(r => r.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(r => r.Lancamento)
            .WithMany()
            .HasForeignKey(r => r.LancamentoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(r => r.CriadoPorUsuario)
            .WithMany()
            .HasForeignKey(r => r.CriadoPor)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(r => r.SessaoId);
        builder.HasIndex(r => r.PacienteId);
    }
}
