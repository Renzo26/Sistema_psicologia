using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.PlanosConta.Commands.CriarPlanoConta;
using PsicoFinance.Application.Features.PlanosConta.DTOs;

namespace PsicoFinance.Application.Features.PlanosConta.Queries.ObterPlanoConta;

public class ObterPlanoContaQueryHandler : IRequestHandler<ObterPlanoContaQuery, PlanoContaDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public ObterPlanoContaQueryHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<PlanoContaDto> Handle(ObterPlanoContaQuery request, CancellationToken cancellationToken)
    {
        _ = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var plano = await _context.PlanosConta
            .AsNoTracking()
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Plano de conta não encontrado.");

        return CriarPlanoContaCommandHandler.MapToDto(plano);
    }
}
