using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Fechamentos.DTOs;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Fechamentos.Commands.RealizarFechamentoMensal;

public class RealizarFechamentoMensalCommandHandler
    : IRequestHandler<RealizarFechamentoMensalCommand, FechamentoDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public RealizarFechamentoMensalCommandHandler(
        IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<FechamentoDto> Handle(
        RealizarFechamentoMensalCommand request, CancellationToken cancellationToken)
    {
        var clinicaId = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        if (!DateOnly.TryParseExact(request.MesReferencia + "-01", "yyyy-MM-dd", out var mesInicio))
            throw new ArgumentException("Formato de mês inválido. Use YYYY-MM.");

        // Verifica se já existe fechamento para o mês
        var jaFechado = await _context.FechamentosMensais
            .AnyAsync(f => f.MesReferencia == request.MesReferencia
                        && f.Status == StatusFechamento.Fechado, cancellationToken);
        if (jaFechado)
            throw new InvalidOperationException($"O mês {request.MesReferencia} já foi fechado.");

        var mesFim = mesInicio.AddMonths(1).AddDays(-1);

        // Totais de lançamentos
        var lancamentos = await _context.LancamentosFinanceiros
            .AsNoTracking()
            .Where(l => l.Competencia == request.MesReferencia
                     && l.Status != StatusLancamento.Cancelado)
            .ToListAsync(cancellationToken);

        var totalReceitas = lancamentos
            .Where(l => l.Tipo == TipoLancamento.Receita && l.Status == StatusLancamento.Confirmado)
            .Sum(l => l.Valor);

        var totalDespesas = lancamentos
            .Where(l => l.Tipo == TipoLancamento.Despesa && l.Status == StatusLancamento.Confirmado)
            .Sum(l => l.Valor);

        // Totais de sessões
        var sessoes = await _context.Sessoes
            .AsNoTracking()
            .Include(s => s.Contrato)
            .Include(s => s.Psicologo)
            .Where(s => s.Data >= mesInicio && s.Data <= mesFim
                     && s.Status != StatusSessao.Cancelada)
            .ToListAsync(cancellationToken);

        var totalSessoes = sessoes.Count;
        var realizadas = sessoes.Count(s => s.Status == StatusSessao.Realizada);
        var faltas = sessoes.Count(s => s.Status is StatusSessao.Falta or StatusSessao.FaltaJustificada);

        // Relatório consolidado por psicólogo
        var repasses = await _context.Repasses
            .AsNoTracking()
            .Include(r => r.Psicologo)
            .Where(r => r.MesReferencia == request.MesReferencia)
            .ToListAsync(cancellationToken);

        var porPsicologo = sessoes
            .Where(s => s.Status == StatusSessao.Realizada)
            .GroupBy(s => new { s.PsicologoId, s.Psicologo.Nome })
            .Select(g =>
            {
                var repasse = repasses.FirstOrDefault(r => r.PsicologoId == g.Key.PsicologoId);
                return new FechamentoRepasseConsolidadoDto(
                    g.Key.PsicologoId,
                    g.Key.Nome,
                    g.Count(),
                    g.Sum(s => s.Contrato.ValorSessao),
                    repasse?.ValorCalculado ?? 0m);
            })
            .ToList();

        // Cria ou atualiza o registro de fechamento
        var fechamento = await _context.FechamentosMensais
            .FirstOrDefaultAsync(f => f.MesReferencia == request.MesReferencia, cancellationToken);

        if (fechamento is null)
        {
            fechamento = new FechamentoMensal
            {
                Id = Guid.NewGuid(),
                ClinicaId = clinicaId,
                MesReferencia = request.MesReferencia,
            };
            _context.FechamentosMensais.Add(fechamento);
        }

        fechamento.Status = StatusFechamento.Fechado;
        fechamento.TotalReceitas = totalReceitas;
        fechamento.TotalDespesas = totalDespesas;
        fechamento.Saldo = totalReceitas - totalDespesas;
        fechamento.TotalSessoes = totalSessoes;
        fechamento.TotalSessoesRealizadas = realizadas;
        fechamento.TotalSessoesFalta = faltas;
        fechamento.FechadoEm = DateTimeOffset.UtcNow;
        fechamento.Observacao = request.Observacao;

        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(fechamento, porPsicologo);
    }

    internal static FechamentoDto MapToDto(
        FechamentoMensal f, List<FechamentoRepasseConsolidadoDto> porPsicologo) => new(
        f.Id, f.MesReferencia, f.Status,
        f.TotalReceitas, f.TotalDespesas, f.Saldo,
        f.TotalSessoes, f.TotalSessoesRealizadas, f.TotalSessoesFalta,
        f.FechadoEm, f.Observacao, porPsicologo);
}
