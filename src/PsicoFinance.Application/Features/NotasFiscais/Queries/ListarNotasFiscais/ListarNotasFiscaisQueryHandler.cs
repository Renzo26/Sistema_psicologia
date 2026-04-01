using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.NotasFiscais.DTOs;

namespace PsicoFinance.Application.Features.NotasFiscais.Queries.ListarNotasFiscais;

public class ListarNotasFiscaisQueryHandler : IRequestHandler<ListarNotasFiscaisQuery, List<NotaFiscalDto>>
{
    private readonly IAppDbContext _context;

    public ListarNotasFiscaisQueryHandler(IAppDbContext context) => _context = context;

    public async Task<List<NotaFiscalDto>> Handle(ListarNotasFiscaisQuery request, CancellationToken cancellationToken)
    {
        var query = _context.NotasFiscais
            .Include(n => n.Paciente)
            .AsNoTracking()
            .AsQueryable();

        if (request.PacienteId.HasValue)
            query = query.Where(n => n.PacienteId == request.PacienteId.Value);

        if (request.CompetenciaInicio.HasValue)
            query = query.Where(n => n.Competencia >= request.CompetenciaInicio.Value);

        if (request.CompetenciaFim.HasValue)
            query = query.Where(n => n.Competencia <= request.CompetenciaFim.Value);

        if (request.Status.HasValue)
            query = query.Where(n => n.Status == request.Status.Value);

        var notas = await query
            .OrderByDescending(n => n.CriadoEm)
            .ToListAsync(cancellationToken);

        return notas.Select(n => new NotaFiscalDto(
            n.Id, n.NumeroNfse, n.PacienteId, n.Paciente.Nome,
            n.ValorServico, n.DescricaoServico, n.Competencia,
            n.DataEmissao, n.Status, n.ErroMensagem,
            n.PdfUrl, n.CriadoEm)).ToList();
    }
}
