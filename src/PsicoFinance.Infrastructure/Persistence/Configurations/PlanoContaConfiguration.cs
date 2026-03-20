using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Infrastructure.Persistence.Configurations;

public class PlanoContaConfiguration : IEntityTypeConfiguration<PlanoConta>
{
    public void Configure(EntityTypeBuilder<PlanoConta> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nome).HasMaxLength(100).IsRequired();
        builder.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(10);
        builder.Property(p => p.Descricao).HasMaxLength(300);

        builder.HasIndex(p => new { p.ClinicaId, p.Nome, p.Tipo }).IsUnique();

        builder.HasOne(p => p.Clinica)
               .WithMany(c => c.PlanosConta)
               .HasForeignKey(p => p.ClinicaId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
