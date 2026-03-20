using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Infrastructure.Persistence.Configurations;

public class PacienteConfiguration : IEntityTypeConfiguration<Paciente>
{
    public void Configure(EntityTypeBuilder<Paciente> builder)
    {
        builder.HasKey(p => p.Id);
        builder.Property(p => p.Nome).HasMaxLength(150).IsRequired();
        builder.Property(p => p.ResponsavelNome).HasMaxLength(150);

        builder.HasOne(p => p.Clinica)
               .WithMany(c => c.Pacientes)
               .HasForeignKey(p => p.ClinicaId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
