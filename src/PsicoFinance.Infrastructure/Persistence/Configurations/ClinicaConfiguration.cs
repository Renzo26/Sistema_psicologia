using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Infrastructure.Persistence.Configurations;

public class ClinicaConfiguration : IEntityTypeConfiguration<Clinica>
{
    public void Configure(EntityTypeBuilder<Clinica> builder)
    {
        builder.HasKey(c => c.Id);
        builder.Property(c => c.Nome).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Cnpj).HasMaxLength(18);
        builder.Property(c => c.Email).HasMaxLength(150).IsRequired();
        builder.Property(c => c.Telefone).HasMaxLength(20);
        builder.Property(c => c.Cep).HasMaxLength(9);
        builder.Property(c => c.Logradouro).HasMaxLength(200);
        builder.Property(c => c.Numero).HasMaxLength(20);
        builder.Property(c => c.Complemento).HasMaxLength(100);
        builder.Property(c => c.Bairro).HasMaxLength(100);
        builder.Property(c => c.Cidade).HasMaxLength(100);
        builder.Property(c => c.Estado).HasMaxLength(2).IsFixedLength();
        builder.Property(c => c.WebhookN8nUrl).HasMaxLength(500);
        builder.Property(c => c.Timezone).HasMaxLength(50).HasDefaultValue("America/Sao_Paulo");

        builder.HasIndex(c => c.Cnpj).IsUnique().HasFilter("cnpj IS NOT NULL");
    }
}
