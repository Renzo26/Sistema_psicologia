using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Fechamentos.DTOs;
using PsicoFinance.Application.Features.Fechamentos.Commands.RealizarFechamentoMensal;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Fechamentos.Queries.ObterFechamento;

public class ObterFechamentoQueryHandler : IRequestHandler<ObterFechamentoQuery, FechamentoDto>
{
    private readonly IAppDbContext _context;

    public ObterFechamentoQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<FechamentoDto> Handle(
        ObterFechamentoQuery request, CancellationToken cancellationToken)
    {
        var fechamento = await _context.FechamentosMensais
            .AsNoTracking()
            .FirstOrDefaultAsync(f => f.MesReferencia == request.MesReferencia, cancellationToken)
            ?? throw new KeyNotFoundException($"Fechamento do mês {request.MesReferencia} não encontrado.");

        if (!DateOnly.TryParseExact(request.MesReferencia + "-01", "yyyy-MM-dd", out var mesInicio))
            throw new ArgumentException("Formato de mês inválido.");

        var mesFim = mesInicio.AddMonths(1).AddDays(-1);

        // Consolida por psicólogo (tempo real)
        var sessoes = await _context.Sessoes
            .AsNoTracking()
            .Include(s => s.Contrato)
            .Include(s => s.Psicologo)
            .Where(s => s.Data >= mesInicio && s.Data <= mesFim
                     && s.Status == StatusSessao.Realizada)
            .ToListAsync(cancellationToken);

        var repasses = await _context.Repasses
            .AsNoTracking()
            .Include(r => r.Psicologo)
            .Where(r => r.MesReferencia == request.MesReferencia)
            .ToListAsync(cancellationToken);

        var porPsicologo = sessoes
            .GroupBy(s => new { s.PsicologoId, s.Psicologo.Nome })
            .Select(g =>
            {
                var repasse = repasses.FirstOrDefault(r => r.PsicologoId == g.Key.PsicologoId);
                return new FechamentoRepasseConsolidadoDto(
                    g.Key.PsicologoId, g.Key.Nome,
                    g.Count(),
                    g.Sum(s => s.Contrato.ValorSessao),
                    repasse?.ValorCalculado ?? 0m);
            })
            .ToList();

        return RealizarFechamentoMensalCommandHandler.MapToDto(fechamento, porPsicologo);
    }
}
