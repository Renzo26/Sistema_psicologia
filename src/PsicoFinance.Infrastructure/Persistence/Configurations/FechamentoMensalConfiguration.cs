using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Infrastructure.Persistence.Configurations;

public class FechamentoMensalConfiguration : IEntityTypeConfiguration<FechamentoMensal>
{
    public void Configure(EntityTypeBuilder<FechamentoMensal> builder)
    {
        builder.HasKey(f => f.Id);
        builder.Property(f => f.MesReferencia).HasMaxLength(7).IsRequired();
        builder.Property(f => f.Status).HasConversion<string>().HasMaxLength(10);
        builder.Property(f => f.TotalReceitas).HasPrecision(14, 2);
        builder.Property(f => f.TotalDespesas).HasPrecision(14, 2);
        builder.Property(f => f.Saldo).HasPrecision(14, 2);
        builder.Property(f => f.Observacao).HasMaxLength(1000);

        builder.HasOne(f => f.Clinica)
               .WithMany()
               .HasForeignKey(f => f.ClinicaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasIndex(f => new { f.ClinicaId, f.MesReferencia }).IsUnique();
    }
}
