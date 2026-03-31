using MediatR;

namespace PsicoFinance.Domain.Events;

public record SessaoRealizadaEvent(Guid SessaoId, Guid ClinicaId) : INotification;
