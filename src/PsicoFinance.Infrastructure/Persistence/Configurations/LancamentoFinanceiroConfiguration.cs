using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Infrastructure.Persistence.Configurations;

public class LancamentoFinanceiroConfiguration : IEntityTypeConfiguration<LancamentoFinanceiro>
{
    public void Configure(EntityTypeBuilder<LancamentoFinanceiro> builder)
    {
        builder.HasKey(l => l.Id);

        builder.Property(l => l.Descricao).HasMaxLength(200).IsRequired();
        builder.Property(l => l.Valor).HasPrecision(18, 2).IsRequired();
        builder.Property(l => l.Tipo).HasConversion<string>().HasMaxLength(10).IsRequired();
        builder.Property(l => l.Status).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(l => l.Competencia).HasMaxLength(7).IsRequired();
        builder.Property(l => l.Observacao).HasMaxLength(500);

        builder.HasIndex(l => new { l.ClinicaId, l.Competencia, l.Status });
        builder.HasIndex(l => new { l.ClinicaId, l.DataVencimento });
        builder.HasIndex(l => l.SessaoId);

        builder.HasOne(l => l.Clinica)
               .WithMany()
               .HasForeignKey(l => l.ClinicaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(l => l.Sessao)
               .WithMany()
               .HasForeignKey(l => l.SessaoId)
               .OnDelete(DeleteBehavior.SetNull);

        builder.HasOne(l => l.PlanoConta)
               .WithMany()
               .HasForeignKey(l => l.PlanoContaId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
