using MediatR;

namespace PsicoFinance.Domain.Events;

public record SessaoCanceladaEvent(Guid SessaoId, Guid ClinicaId) : INotification;
