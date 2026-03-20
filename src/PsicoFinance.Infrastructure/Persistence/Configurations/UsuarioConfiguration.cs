using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Infrastructure.Persistence.Configurations;

public class UsuarioConfiguration : IEntityTypeConfiguration<Usuario>
{
    public void Configure(EntityTypeBuilder<Usuario> builder)
    {
        builder.HasKey(u => u.Id);
        builder.Property(u => u.Nome).HasMaxLength(150).IsRequired();
        builder.Property(u => u.Email).HasMaxLength(150).IsRequired();
        builder.Property(u => u.SenhaHash).HasMaxLength(255).IsRequired();
        builder.Property(u => u.Role).HasConversion<string>().HasMaxLength(20);

        builder.HasIndex(u => new { u.ClinicaId, u.Email }).IsUnique();
        builder.HasIndex(u => u.Email);

        builder.HasOne(u => u.Clinica)
               .WithMany(c => c.Usuarios)
               .HasForeignKey(u => u.ClinicaId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(u => u.Psicologo)
               .WithMany(p => p.Usuarios)
               .HasForeignKey(u => u.PsicologoId)
               .OnDelete(DeleteBehavior.SetNull);
    }
}
