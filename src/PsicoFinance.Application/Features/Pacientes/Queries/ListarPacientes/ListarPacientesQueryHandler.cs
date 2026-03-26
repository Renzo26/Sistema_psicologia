using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Pacientes.DTOs;

namespace PsicoFinance.Application.Features.Pacientes.Queries.ListarPacientes;

public class ListarPacientesQueryHandler : IRequestHandler<ListarPacientesQuery, List<PacienteResumoDto>>
{
    private readonly IAppDbContext _context;

    public ListarPacientesQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<List<PacienteResumoDto>> Handle(ListarPacientesQuery request, CancellationToken cancellationToken)
    {
        var query = _context.Pacientes.AsNoTracking();

        if (request.ApenasAtivos == true)
            query = query.Where(p => p.Ativo);

        if (!string.IsNullOrWhiteSpace(request.Busca))
        {
            var busca = request.Busca.ToLower();
            query = query.Where(p =>
                p.Nome.ToLower().Contains(busca) ||
                (p.Cpf != null && p.Cpf.Contains(busca)));
        }

        return await query
            .OrderBy(p => p.Nome)
            .Select(p => new PacienteResumoDto(
                p.Id, p.Nome, p.Cpf, p.Telefone, p.Ativo))
            .ToListAsync(cancellationToken);
    }
}
