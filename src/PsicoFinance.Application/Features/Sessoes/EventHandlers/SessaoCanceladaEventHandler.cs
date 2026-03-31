using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Domain.Events;

namespace PsicoFinance.Application.Features.Sessoes.EventHandlers;

/// <summary>
/// Ao cancelar uma sessão, cancela o lançamento financeiro vinculado (se Previsto).
/// </summary>
public class SessaoCanceladaEventHandler : INotificationHandler<SessaoCanceladaEvent>
{
    private readonly IAppDbContext _context;

    public SessaoCanceladaEventHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SessaoCanceladaEvent notification, CancellationToken cancellationToken)
    {
        var lancamento = await _context.LancamentosFinanceiros
            .FirstOrDefaultAsync(
                l => l.SessaoId == notification.SessaoId
                  && l.Status == StatusLancamento.Previsto,
                cancellationToken);

        if (lancamento is null) return;

        lancamento.Status = StatusLancamento.Cancelado;

        await _context.SaveChangesAsync(cancellationToken);
    }
}
