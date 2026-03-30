using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Sessoes.Commands.CancelarSessao;

public class CancelarSessaoCommandHandler : IRequestHandler<CancelarSessaoCommand>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public CancelarSessaoCommandHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(CancelarSessaoCommand request, CancellationToken cancellationToken)
    {
        var sessao = await _context.Sessoes
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Sessão não encontrada.");

        if (sessao.Status == StatusSessao.Cancelada)
            throw new InvalidOperationException("Sessão já está cancelada.");

        // Regra: só pode alterar status dentro de 30 dias (exceto Admin)
        var isAdmin = _tenantProvider.UserRole == "Admin";
        var limiteDias = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        if (!isAdmin && sessao.Data < limiteDias)
            throw new InvalidOperationException("Não é permitido alterar o status de sessões com mais de 30 dias.");

        sessao.Status = StatusSessao.Cancelada;
        sessao.MotivoFalta = request.Motivo;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
