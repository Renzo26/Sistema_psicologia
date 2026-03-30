using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Repasses.Commands.GerarRepasseMensal;
using PsicoFinance.Application.Features.Repasses.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Repasses.Commands.PagarRepasse;

public class PagarRepasseCommandHandler : IRequestHandler<PagarRepasseCommand, RepasseDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public PagarRepasseCommandHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<RepasseDto> Handle(PagarRepasseCommand request, CancellationToken cancellationToken)
    {
        _ = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var repasse = await _context.Repasses
            .Include(r => r.Psicologo)
            .FirstOrDefaultAsync(r => r.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Repasse não encontrado.");

        if (repasse.Status == StatusRepasse.Pago)
            throw new InvalidOperationException("Repasse já está pago.");

        if (repasse.Status == StatusRepasse.Cancelado)
            throw new InvalidOperationException("Não é possível pagar um repasse cancelado.");

        repasse.Status = StatusRepasse.Pago;
        repasse.DataPagamento = request.DataPagamento;
        repasse.Observacao = request.Observacao;

        await _context.SaveChangesAsync(cancellationToken);

        return GerarRepasseMensalCommandHandler.MapToDto(repasse, repasse.Psicologo.Nome);
    }
}
