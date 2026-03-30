using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.PlanosConta.Commands.CriarPlanoConta;
using PsicoFinance.Application.Features.PlanosConta.DTOs;

namespace PsicoFinance.Application.Features.PlanosConta.Commands.AtualizarPlanoConta;

public class AtualizarPlanoContaCommandHandler : IRequestHandler<AtualizarPlanoContaCommand, PlanoContaDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public AtualizarPlanoContaCommandHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task<PlanoContaDto> Handle(AtualizarPlanoContaCommand request, CancellationToken cancellationToken)
    {
        _ = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        var plano = await _context.PlanosConta
            .FirstOrDefaultAsync(p => p.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Plano de conta não encontrado.");

        var duplicado = await _context.PlanosConta
            .AnyAsync(p => p.Nome == request.Nome && p.Tipo == request.Tipo && p.Id != request.Id, cancellationToken);

        if (duplicado)
            throw new InvalidOperationException("Já existe outro plano de conta com este nome e tipo.");

        plano.Nome = request.Nome;
        plano.Tipo = request.Tipo;
        plano.Descricao = request.Descricao;
        plano.Ativo = request.Ativo;

        await _context.SaveChangesAsync(cancellationToken);

        return CriarPlanoContaCommandHandler.MapToDto(plano);
    }
}
