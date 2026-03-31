using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Domain.Events;

namespace PsicoFinance.Application.Features.Sessoes.EventHandlers;

/// <summary>
/// Ao marcar sessão como Realizada, confirma o lançamento financeiro vinculado.
/// </summary>
public class SessaoRealizadaEventHandler : INotificationHandler<SessaoRealizadaEvent>
{
    private readonly IAppDbContext _context;

    public SessaoRealizadaEventHandler(IAppDbContext context)
    {
        _context = context;
    }

    public async Task Handle(SessaoRealizadaEvent notification, CancellationToken cancellationToken)
    {
        var lancamento = await _context.LancamentosFinanceiros
            .FirstOrDefaultAsync(
                l => l.SessaoId == notification.SessaoId
                  && l.Status == StatusLancamento.Previsto,
                cancellationToken);

        if (lancamento is null) return;

        lancamento.Status = StatusLancamento.Confirmado;
        lancamento.DataPagamento = DateOnly.FromDateTime(DateTime.UtcNow);

        await _context.SaveChangesAsync(cancellationToken);
    }
}
