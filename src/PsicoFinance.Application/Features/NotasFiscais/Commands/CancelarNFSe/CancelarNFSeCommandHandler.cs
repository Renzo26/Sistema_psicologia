using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.NotasFiscais.Commands.CancelarNFSe;

public class CancelarNFSeCommandHandler : IRequestHandler<CancelarNFSeCommand>
{
    private readonly IAppDbContext _context;

    public CancelarNFSeCommandHandler(IAppDbContext context) => _context = context;

    public async Task Handle(CancelarNFSeCommand request, CancellationToken cancellationToken)
    {
        var nota = await _context.NotasFiscais
            .FirstOrDefaultAsync(n => n.Id == request.NotaFiscalId, cancellationToken)
            ?? throw new KeyNotFoundException("Nota fiscal não encontrada.");

        if (nota.Status == StatusNfse.Cancelada)
            throw new InvalidOperationException("Nota fiscal já está cancelada.");

        nota.Status = StatusNfse.Cancelada;
        await _context.SaveChangesAsync(cancellationToken);
    }
}
