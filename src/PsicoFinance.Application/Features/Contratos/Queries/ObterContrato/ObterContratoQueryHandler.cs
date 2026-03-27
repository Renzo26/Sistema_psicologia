using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Contratos.DTOs;

namespace PsicoFinance.Application.Features.Contratos.Queries.ObterContrato;

public class ObterContratoQueryHandler : IRequestHandler<ObterContratoQuery, ContratoDto>
{
    private readonly IAppDbContext _context;

    public ObterContratoQueryHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task<ContratoDto> Handle(ObterContratoQuery request, CancellationToken cancellationToken)
    {
        var c = await _context.Contratos
            .AsNoTracking()
            .Include(c => c.Paciente)
            .Include(c => c.Psicologo)
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Contrato não encontrado.");

        return new ContratoDto(
            c.Id, c.PacienteId, c.Paciente.Nome,
            c.PsicologoId, c.Psicologo.Nome,
            c.ValorSessao, c.FormaPagamento.ToString(),
            c.Frequencia.ToString(), c.DiaSemanasessao.ToString(),
            c.HorarioSessao, c.DuracaoMinutos,
            c.CobraFaltaInjustificada, c.CobraFaltaJustificada,
            c.DataInicio, c.DataFim, c.Status.ToString(),
            c.MotivoEncerramento, c.PlanoContaId,
            c.Observacoes, c.CriadoEm);
    }
}
