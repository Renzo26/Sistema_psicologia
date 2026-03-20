using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Infrastructure.Persistence.Configurations;

public class PsicologoConfiguration : IEntityTypeConfiguration<Psicologo>
{
    public void Configure(EntityTypeBuilder<Psicologo> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nome).HasMaxLength(150).IsRequired();
        builder.Property(p => p.Crp).HasMaxLength(20).IsRequired();
        builder.Property(p => p.Email).HasMaxLength(150);
        builder.Property(p => p.Telefone).HasMaxLength(20);
        builder.Property(p => p.Cpf).HasMaxLength(14);
        builder.Property(p => p.Tipo).HasConversion<string>().HasMaxLength(10);
        builder.Property(p => p.TipoRepasse).HasConversion<string>().HasMaxLength(20);
        builder.Property(p => p.ValorRepasse).HasPrecision(10, 2);
        builder.Property(p => p.Banco).HasMaxLength(100);
        builder.Property(p => p.Agencia).HasMaxLength(20);
        builder.Property(p => p.Conta).HasMaxLength(30);

        builder.HasIndex(p => new { p.ClinicaId, p.Crp }).IsUnique();

        builder.HasOne(p => p.Clinica)
               .WithMany(c => c.Psicologos)
               .HasForeignKey(p => p.ClinicaId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
