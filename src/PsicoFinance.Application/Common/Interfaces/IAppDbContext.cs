using Microsoft.EntityFrameworkCore;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Application.Common.Interfaces;

public interface IAppDbContext
{
    DbSet<Usuario> Usuarios { get; }
    DbSet<RefreshToken> RefreshTokens { get; }
    DbSet<Clinica> Clinicas { get; }
    DbSet<Psicologo> Psicologos { get; }
    DbSet<Paciente> Pacientes { get; }
    DbSet<Contrato> Contratos { get; }
    DbSet<PlanoConta> PlanosConta { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
