using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Sessoes.Commands.RegistrarFalta;

public class RegistrarFaltaCommandHandler : IRequestHandler<RegistrarFaltaCommand>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;

    public RegistrarFaltaCommandHandler(IAppDbContext context, ITenantProvider tenantProvider)
    {
        _context = context;
        _tenantProvider = tenantProvider;
    }

    public async Task Handle(RegistrarFaltaCommand request, CancellationToken cancellationToken)
    {
        var sessao = await _context.Sessoes
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Sessão não encontrada.");

        if (sessao.Status == StatusSessao.Cancelada)
            throw new InvalidOperationException("Não é possível registrar falta em sessão cancelada.");

        var isAdmin = _tenantProvider.UserRole == "Admin";
        var limiteDias = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        if (!isAdmin && sessao.Data < limiteDias)
            throw new InvalidOperationException("Não é permitido alterar o status de sessões com mais de 30 dias.");

        sessao.Status = request.Justificada ? StatusSessao.FaltaJustificada : StatusSessao.Falta;
        sessao.MotivoFalta = request.Motivo;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
