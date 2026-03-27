using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Contratos.Commands.EncerrarContrato;

public class EncerrarContratoCommandHandler : IRequestHandler<EncerrarContratoCommand>
{
    private readonly IAppDbContext _context;

    public EncerrarContratoCommandHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(EncerrarContratoCommand request, CancellationToken cancellationToken)
    {
        var contrato = await _context.Contratos
            .FirstOrDefaultAsync(c => c.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Contrato não encontrado.");

        if (contrato.Status == StatusContrato.Encerrado)
            throw new InvalidOperationException("Contrato já está encerrado.");

        contrato.Status = StatusContrato.Encerrado;
        contrato.MotivoEncerramento = request.MotivoEncerramento;
        contrato.DataFim = DateOnly.FromDateTime(DateTime.Today);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
