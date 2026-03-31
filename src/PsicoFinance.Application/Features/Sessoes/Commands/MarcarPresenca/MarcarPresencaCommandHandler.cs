using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Domain.Enums;
using PsicoFinance.Domain.Events;

namespace PsicoFinance.Application.Features.Sessoes.Commands.MarcarPresenca;

public class MarcarPresencaCommandHandler : IRequestHandler<MarcarPresencaCommand>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPublisher _publisher;

    public MarcarPresencaCommandHandler(
        IAppDbContext context, ITenantProvider tenantProvider, IPublisher publisher)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _publisher = publisher;
    }

    public async Task Handle(MarcarPresencaCommand request, CancellationToken cancellationToken)
    {
        var sessao = await _context.Sessoes
            .FirstOrDefaultAsync(s => s.Id == request.Id, cancellationToken)
            ?? throw new KeyNotFoundException("Sessão não encontrada.");

        if (sessao.Status == StatusSessao.Realizada)
            throw new InvalidOperationException("Presença já foi marcada para esta sessão.");

        if (sessao.Status == StatusSessao.Cancelada)
            throw new InvalidOperationException("Não é possível marcar presença em sessão cancelada.");

        var isAdmin = _tenantProvider.UserRole == "Admin";
        var limiteDias = DateOnly.FromDateTime(DateTime.UtcNow.AddDays(-30));
        if (!isAdmin && sessao.Data < limiteDias)
            throw new InvalidOperationException("Não é permitido alterar o status de sessões com mais de 30 dias.");

        sessao.Status = StatusSessao.Realizada;
        sessao.MotivoFalta = null;

        await _context.SaveChangesAsync(cancellationToken);

        await _publisher.Publish(
            new SessaoRealizadaEvent(sessao.Id, sessao.ClinicaId), cancellationToken);
    }
}
