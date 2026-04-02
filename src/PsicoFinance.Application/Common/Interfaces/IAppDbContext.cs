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
    DbSet<Sessao> Sessoes { get; }
    DbSet<PlanoConta> PlanosConta { get; }
    DbSet<LancamentoFinanceiro> LancamentosFinanceiros { get; }
    DbSet<Repasse> Repasses { get; }
    DbSet<FechamentoMensal> FechamentosMensais { get; }
    DbSet<AuditLog> AuditLogs { get; }
    DbSet<Recibo> Recibos { get; }
    DbSet<NotaFiscal> NotasFiscais { get; }
    DbSet<RelatorioPersonalizado> RelatoriosPersonalizados { get; }
    Task<int> SaveChangesAsync(CancellationToken cancellationToken = default);
}
