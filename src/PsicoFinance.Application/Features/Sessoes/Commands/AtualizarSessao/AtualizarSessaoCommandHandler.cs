using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Sessoes.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Sessoes.Commands.AtualizarSessao;

public class AtualizarSessaoCommandHandler : IRequestHandler<AtualizarSessaoCommand, SessaoDto>
{
    private readonly IAppDbContext _context;

    public AtualizarSessaoCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<SessaoDto> Handle(AtualizarSessaoCommand request, CancellationToken cancellationToken)
    {
        var sessao = await _context.Sessoes
            .Include(s => s.Paciente)
            .Include(s => s.Psicologo)
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Sessão não encontrada.");

        if (sessao.Status != StatusSessao.Agendada)
            throw new InvalidOperationException("Apenas sessões com status 'Agendada' podem ser editadas.");

        sessao.Data = request.Data;
        sessao.HorarioInicio = request.HorarioInicio;
        sessao.DuracaoMinutos = request.DuracaoMinutos;
        sessao.Observacoes = request.Observacoes;

        await _context.SaveChangesAsync(cancellationToken);

        return AgendarSessao.AgendarSessaoCommandHandler.MapToDto(
            sessao, sessao.Paciente.Nome, sessao.Psicologo.Nome);
    }
}
