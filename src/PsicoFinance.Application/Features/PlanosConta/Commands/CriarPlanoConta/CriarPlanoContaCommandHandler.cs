using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.PlanosConta.DTOs;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Application.Features.PlanosConta.Commands.CriarPlanoConta;

public class CriarPlanoContaCommandHandler : IRequestHandler<CriarPlanoContaCommand, PlanoContaDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CriarPlanoContaCommandHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<PlanoContaDto> Handle(CriarPlanoContaCommand request, CancellationToken cancellationToken)
    {
        var clinicaId = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var duplicado = await _context.PlanosConta
            .AnyAsync(p => p.Nome == request.Nome && p.Tipo == request.Tipo, cancellationToken);

        if (duplicado)
            throw new InvalidOperationException("Já existe um plano de conta com este nome e tipo.");

        var plano = new PlanoConta
        {
            Id = Guid.NewGuid(),
            ClinicaId = clinicaId,
            Nome = request.Nome,
            Tipo = request.Tipo,
            Descricao = request.Descricao,
            Ativo = true
        };

        _context.PlanosConta.Add(plano);
        await _context.SaveChangesAsync(cancellationToken);

        return MapToDto(plano);
    }

    internal static PlanoContaDto MapToDto(PlanoConta p) => new(
        p.Id, p.Nome, p.Tipo, p.Descricao, p.Ativo, p.CriadoEm);
}
