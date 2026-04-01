using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Relatorios.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Relatorios.Commands.GerarRelatorioMensal;

public class GerarRelatorioMensalCommandHandler : IRequestHandler<GerarRelatorioMensalCommand, RelatorioMensalDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPdfService _pdfService;
    private readonly IFileStorageService _storageService;

    public GerarRelatorioMensalCommandHandler(
        IAppDbContext context,
        ITenantProvider tenantProvider,
        IPdfService pdfService,
        IFileStorageService storageService)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _pdfService = pdfService;
        _storageService = storageService;
    }

    public async Task<RelatorioMensalDto> Handle(GerarRelatorioMensalCommand request, CancellationToken cancellationToken)
    {
        var clinicaId = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        // Busca dados do psicólogo
        var psicologo = await _context.Psicologos
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.PsicologoId, cancellationToken)
            ?? throw new KeyNotFoundException("Psicólogo não encontrado.");

        // Busca dados da clínica
        var clinica = await _context.Clinicas
            .AsNoTracking()
            .FirstOrDefaultAsync(c => c.Id == clinicaId, cancellationToken)
            ?? throw new KeyNotFoundException("Clínica não encontrada.");

        // Determina o período da competência
        var ano = int.Parse(request.Competencia[..4]);
        var mes = int.Parse(request.Competencia[5..]);
        var inicio = new DateOnly(ano, mes, 1);
        var fim = inicio.AddMonths(1).AddDays(-1);

        // Busca sessões do psicólogo no período
        var sessoes = await _context.Sessoes
            .Include(s => s.Paciente)
            .Include(s => s.Contrato)
            .AsNoTracking()
            .Where(s => s.PsicologoId == request.PsicologoId
                && s.Data >= inicio
                && s.Data <= fim)
            .OrderBy(s => s.Data)
            .ThenBy(s => s.HorarioInicio)
            .ToListAsync(cancellationToken);

        var realizadas = sessoes.Count(s => s.Status == StatusSessao.Realizada);
        var faltas = sessoes.Count(s => s.Status == StatusSessao.Falta || s.Status == StatusSessao.FaltaJustificada);
        var canceladas = sessoes.Count(s => s.Status == StatusSessao.Cancelada);

        // Calcula receita total (sessões realizadas)
        var receitaTotal = sessoes
            .Where(s => s.Status == StatusSessao.Realizada)
            .Sum(s => s.Contrato.ValorSessao);

        // Calcula repasse
        decimal valorRepasse;
        if (psicologo.TipoRepasse == TipoRepasse.Percentual)
            valorRepasse = receitaTotal * (psicologo.ValorRepasse / 100m);
        else
            valorRepasse = realizadas * psicologo.ValorRepasse;

        // Monta dados para o PDF
        var sessaoItems = sessoes.Select(s => new SessaoRelatorioItem(
            s.Data,
            s.HorarioInicio,
            s.Paciente.Nome,
            s.Status.ToString(),
            s.Contrato.ValorSessao
        )).ToList();

        var relatorioData = new RelatorioMensalData(
            ClinicaNome: clinica.Nome,
            ClinicaCnpj: clinica.Cnpj,
            PsicologoNome: psicologo.Nome,
            PsicologoCrp: psicologo.Crp,
            Competencia: request.Competencia,
            Sessoes: sessaoItems,
            TotalRealizadas: realizadas,
            TotalFaltas: faltas,
            TotalCanceladas: canceladas,
            ReceitaTotal: receitaTotal,
            ValorRepasse: valorRepasse,
            TipoRepasse: psicologo.TipoRepasse.ToString(),
            PercentualOuValorRepasse: psicologo.ValorRepasse
        );

        var pdfBytes = _pdfService.GerarRelatorioMensalPsicologo(relatorioData);

        // Salva o arquivo
        var fileName = $"relatorio_{psicologo.Nome.Replace(" ", "_")}_{request.Competencia}.pdf";
        var folder = $"{clinicaId}/relatorios";
        var relativePath = await _storageService.SaveAsync(folder, fileName, pdfBytes, cancellationToken);

        return new RelatorioMensalDto(
            Guid.NewGuid(),
            psicologo.Nome,
            request.Competencia,
            realizadas,
            receitaTotal,
            valorRepasse,
            relativePath,
            DateTimeOffset.UtcNow);
    }
}
