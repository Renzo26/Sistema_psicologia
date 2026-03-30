using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Sessoes.DTOs;
using PsicoFinance.Application.Features.Sessoes.Commands.AgendarSessao;

namespace PsicoFinance.Application.Features.Sessoes.Queries.ObterSessao;

public class ObterSessaoQueryHandler : IRequestHandler<ObterSessaoQuery, SessaoDto>
{
    private readonly IAppDbContext _context;

    public ObterSessaoQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<SessaoDto> Handle(ObterSessaoQuery request, CancellationToken cancellationToken)
    {
        var sessao = await _context.Sessoes
            .AsNoTracking()
            .Include(s => s.Paciente)
            .Include(s => s.Psicologo)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Sessão não encontrada.");

        return AgendarSessaoCommandHandler.MapToDto(
            sessao, sessao.Paciente.Nome, sessao.Psicologo.Nome);
    }
}
