using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Infrastructure.Persistence.Configurations;

public class RepasseConfiguration : IEntityTypeConfiguration<Repasse>
{
    public void Configure(EntityTypeBuilder<Repasse> builder)
    {
        builder.HasKey(r => r.Id);

        builder.Property(r => r.MesReferencia).HasMaxLength(7).IsRequired();
        builder.Property(r => r.ValorCalculado).HasPrecision(18, 2).IsRequired();
        builder.Property(r => r.Status).HasConversion<string>().HasMaxLength(15).IsRequired();
        builder.Property(r => r.Observacao).HasMaxLength(500);

        builder.HasIndex(r => new { r.ClinicaId, r.PsicologoId, r.MesReferencia }).IsUnique();

        builder.HasOne(r => r.Clinica)
               .WithMany()
               .HasForeignKey(r => r.ClinicaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Psicologo)
               .WithMany()
               .HasForeignKey(r => r.PsicologoId)
               .OnDelete(DeleteBehavior.Restrict);
    }
}
