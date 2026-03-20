using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata.Builders;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Infrastructure.Persistence.Configurations;

public class RefreshTokenConfiguration : IEntityTypeConfiguration<RefreshToken>
{
    public void Configure(EntityTypeBuilder<RefreshToken> builder)
    {
        builder.HasKey(r => r.Id);
        builder.Property(r => r.TokenHash).HasMaxLength(255).IsRequired();
        builder.Property(r => r.IpOrigem).HasMaxLength(45);
        builder.Property(r => r.UserAgent).HasMaxLength(500);

        builder.HasIndex(r => r.TokenHash).IsUnique();
        builder.HasIndex(r => r.UsuarioId);
        builder.HasIndex(r => r.ExpiraEm);

        builder.HasOne(r => r.Usuario)
               .WithMany(u => u.RefreshTokens)
               .HasForeignKey(r => r.UsuarioId)
               .OnDelete(DeleteBehavior.Cascade);

        builder.HasOne(r => r.Clinica)
               .WithMany()
               .HasForeignKey(r => r.ClinicaId)
               .OnDelete(DeleteBehavior.Cascade);
    }
}
