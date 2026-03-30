using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Repasses.DTOs;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Repasses.Commands.GerarRepasseMensal;

public class GerarRepasseMensalCommandHandler : IRequestHandler<GerarRepasseMensalCommand, List<RepasseDto>>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public GerarRepasseMensalCommandHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<List<RepasseDto>> Handle(GerarRepasseMensalCommand request, CancellationToken cancellationToken)
    {
        var clinicaId = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        // Carregar psicólogos PJ ativos
        var psicologosQuery = _context.Psicologos
            .AsNoTracking()
            .Where(p => p.Ativo && p.Tipo == TipoPsicologo.Pj);

        if (request.PsicologoId.HasValue)
            psicologosQuery = psicologosQuery.Where(p => p.Id == request.PsicologoId.Value);

        var psicologos = await psicologosQuery.ToListAsync(cancellationToken);

        if (!psicologos.Any())
            return [];

        // Buscar sessões realizadas no mês de referência
        if (!DateOnly.TryParseExact(request.MesReferencia + "-01", "yyyy-MM-dd", out var mesInicio))
            throw new ArgumentException("Formato de mês inválido. Use YYYY-MM.");

        var mesFim = mesInicio.AddMonths(1).AddDays(-1);

        var sessoesDoPeriodo = await _context.Sessoes
            .AsNoTracking()
            .Include(s => s.Contrato)
            .Where(s =>
                s.Status == StatusSessao.Realizada &&
                s.Data >= mesInicio &&
                s.Data <= mesFim)
            .ToListAsync(cancellationToken);

        var repasses = new List<RepasseDto>();

        foreach (var psicologo in psicologos)
        {
            // Verificar se já existe repasse para este psicólogo/mês
            var jaExiste = await _context.Repasses
                .AnyAsync(r => r.PsicologoId == psicologo.Id && r.MesReferencia == request.MesReferencia, cancellationToken);

            if (jaExiste)
                continue;

            var sessoesPsicologo = sessoesDoPeriodo
                .Where(s => s.PsicologoId == psicologo.Id)
                .ToList();

            if (!sessoesPsicologo.Any())
                continue;

            var valorCalculado = psicologo.TipoRepasse switch
            {
                TipoRepasse.Percentual => sessoesPsicologo
                    .Sum(s => s.Contrato.ValorSessao * psicologo.ValorRepasse / 100m),
                TipoRepasse.ValorFixo => sessoesPsicologo.Count * psicologo.ValorRepasse,
                _ => 0m
            };

            var repasse = new Repasse
            {
                Id = Guid.NewGuid(),
                ClinicaId = clinicaId,
                PsicologoId = psicologo.Id,
                MesReferencia = request.MesReferencia,
                ValorCalculado = valorCalculado,
                TotalSessoes = sessoesPsicologo.Count,
                Status = StatusRepasse.Pendente
            };

            _context.Repasses.Add(repasse);
            repasses.Add(MapToDto(repasse, psicologo.Nome));
        }

        if (repasses.Any())
            await _context.SaveChangesAsync(cancellationToken);

        return repasses;
    }

    internal static RepasseDto MapToDto(Repasse r, string psicologoNome) => new(
        r.Id, r.PsicologoId, psicologoNome, r.MesReferencia,
        r.ValorCalculado, r.TotalSessoes, r.Status,
        r.DataPagamento, r.Observacao, r.CriadoEm);
}
