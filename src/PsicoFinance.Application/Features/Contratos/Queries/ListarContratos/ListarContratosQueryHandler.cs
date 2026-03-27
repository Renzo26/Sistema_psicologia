using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Contratos.DTOs;

namespace PsicoFinance.Application.Features.Contratos.Queries.ListarContratos;

public class ListarContratosQueryHandler : IRequestHandler<ListarContratosQuery, List<ContratoResumoDto>>
{
    private readonly IAppDbContext _context;

    public ListarContratosQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<ContratoResumoDto>> Handle(ListarContratosQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Contratos
            .AsNoTracking()
            .Include(c => c.Paciente)
            .Include(c => c.Psicologo)
            .AsQueryable();

        if (request.Status.HasValue)
            query = query.Where(c => c.Status == request.Status.Value);

        if (request.PsicologoId.HasValue)
            query = query.Where(c => c.PsicologoId == request.PsicologoId.Value);

        if (request.PacienteId.HasValue)
            query = query.Where(c => c.PacienteId == request.PacienteId.Value);

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var busca = request.Busca.ToLower();
            query = query.Where(c =>
                c.Paciente.Nome.ToLower().Contains(busca) ||
                c.Psicologo.Nome.ToLower().Contains(busca));
        }

        return await query
            .OrderByDescending(c => c.CriadoEm)
            .Select(c => new ContratoResumoDto(
                c.Id,
                c.Paciente.Nome,
                c.Psicologo.Nome,
                c.ValorSessao,
                c.Frequencia.ToString(),
                c.DiaSemanasessao.ToString(),
                c.HorarioSessao,
                c.Status.ToString()))
            .ToListAsync(cancellationToken);
    }
}
