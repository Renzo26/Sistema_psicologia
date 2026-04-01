using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Recibos.Commands.CancelarRecibo;

public class CancelarReciboCommandHandler : IRequestHandler<CancelarReciboCommand>
{
    private readonly IAppDbContext _context;

    public CancelarReciboCommandHandler(IAppDbContext context) => _context = context;

    public async Task Handle(CancelarReciboCommand request, CancellationToken cancellationToken)
    {
        var recibo = await _context.Recibos
            .FirstOrDefaultAsync(r => r.Id == request.ReciboId, cancellationToken)
            ?? throw new KeyNotFoundException("Recibo não encontrado.");

        if (recibo.Status == StatusRecibo.Cancelado)
            throw new InvalidOperationException("Recibo já está cancelado.");

        recibo.Status = StatusRecibo.Cancelado;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
