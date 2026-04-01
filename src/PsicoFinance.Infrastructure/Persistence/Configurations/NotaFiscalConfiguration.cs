using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Infrastructure.Persistence.Configurations;

public class NotaFiscalConfiguration : IEntityTypeConfiguration<NotaFiscal>
{
    public void Configure(EntityTypeBuilder<NotaFiscal> builder)
    {
        builder.HasKey(n => n.Id);

        builder.Property(n => n.NumeroNfse).HasMaxLength(50);
        builder.Property(n => n.CodigoVerificacao).HasMaxLength(100);

        builder.Property(n => n.ValorServico)
            .HasPrecision(10, 2)
            .IsRequired();

        builder.Property(n => n.DescricaoServico)
            .HasDefaultValue("Serviços de Psicologia");

        builder.Property(n => n.Status)
            .HasConversion<string>()
            .HasMaxLength(20);

        builder.Property(n => n.ErroMensagem).HasColumnType("text");
        builder.Property(n => n.XmlUrl).HasMaxLength(1000);
        builder.Property(n => n.PdfUrl).HasMaxLength(1000);
        builder.Property(n => n.RespostaApi).HasColumnType("jsonb");

        builder.HasOne(n => n.Paciente)
            .WithMany()
            .HasForeignKey(n => n.PacienteId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.HasOne(n => n.Lancamento)
            .WithMany()
            .HasForeignKey(n => n.LancamentoId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(n => n.CriadoPorUsuario)
            .WithMany()
            .HasForeignKey(n => n.CriadoPor)
            .OnDelete(DeleteBehavior.SetNull);

        builder.HasIndex(n => n.PacienteId);
        builder.HasIndex(n => n.Status);
        builder.HasIndex(n => n.Competencia);
    }
}
