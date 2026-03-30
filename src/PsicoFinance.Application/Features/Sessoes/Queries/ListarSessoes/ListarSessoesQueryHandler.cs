using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Sessoes.DTOs;

namespace PsicoFinance.Application.Features.Sessoes.Queries.ListarSessoes;

public class ListarSessoesQueryHandler : IRequestHandler<ListarSessoesQuery, List<SessaoResumoDto>>
{
    private readonly IAppDbContext _context;

    public ListarSessoesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<SessaoResumoDto>> Handle(
        ListarSessoesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Sessoes
            .AsNoTracking()
            .Include(s => s.Paciente)
            .Include(s => s.Psicologo)
            .AsQueryable();

        if (request.DataInicio.HasValue)
            query = query.Where(s => s.Data >= request.DataInicio.Value);

        if (request.DataFim.HasValue)
            query = query.Where(s => s.Data <= request.DataFim.Value);

        if (request.PsicologoId.HasValue)
            query = query.Where(s => s.PsicologoId == request.PsicologoId.Value);

        if (request.PacienteId.HasValue)
            query = query.Where(s => s.PacienteId == request.PacienteId.Value);

        if (request.ContratoId.HasValue)
            query = query.Where(s => s.ContratoId == request.ContratoId.Value);

        if (request.Status.HasValue)
            query = query.Where(s => s.Status == request.Status.Value);

        return await query
            .OrderBy(s => s.Data)
            .ThenBy(s => s.HorarioInicio)
            .Select(s => new SessaoResumoDto(
                s.Id, s.ContratoId,
                s.Paciente.Nome, s.Psicologo.Nome,
                s.Data, s.HorarioInicio, s.DuracaoMinutos,
                s.Status.ToString()))
            .ToListAsync(cancellationToken);
    }
}
