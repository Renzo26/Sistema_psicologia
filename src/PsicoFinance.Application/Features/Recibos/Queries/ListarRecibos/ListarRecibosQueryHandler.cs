using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Recibos.DTOs;

namespace PsicoFinance.Application.Features.Recibos.Queries.ListarRecibos;

public class ListarRecibosQueryHandler : IRequestHandler<ListarRecibosQuery, List<ReciboDto>>
{
    private readonly IAppDbContext _context;

    public ListarRecibosQueryHandler(IAppDbContext context) => _context = context;

    public async Task<List<ReciboDto>> Handle(ListarRecibosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Recibos
            .Include(r => r.Paciente)
            .Include(r => r.Sessao)
                .ThenInclude(s => s.Psicologo)
            .AsNoTracking()
            .AsQueryable();

        if (request.PacienteId.HasValue)
            query = query.Where(r => r.PacienteId == request.PacienteId.Value);

        if (request.DataInicio.HasValue)
            query = query.Where(r => r.DataEmissao >= request.DataInicio.Value);

        if (request.DataFim.HasValue)
            query = query.Where(r => r.DataEmissao <= request.DataFim.Value);

        if (request.Status.HasValue)
            query = query.Where(r => r.Status == request.Status.Value);

        var recibos = await query
            .OrderByDescending(r => r.CriadoEm)
            .ToListAsync(cancellationToken);

        return recibos.Select(r => new ReciboDto(
            r.Id, r.NumeroRecibo, r.SessaoId,
            r.Paciente.Nome, r.Sessao.Psicologo.Nome,
            r.Valor, r.DataEmissao, r.Status,
            r.ArquivoUrl, r.CriadoEm)).ToList();
    }
}
