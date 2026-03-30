using MediatR;
using PsicoFinance.Application.Features.Sessoes.DTOs;

namespace PsicoFinance.Application.Features.Sessoes.Commands.AgendarSessao;

public record AgendarSessaoCommand(
    Guid ContratoId,
    DateOnly Data,
    TimeOnly? HorarioInicio,
    int? DuracaoMinutos,
    string? Observacoes) : IRequest<SessaoDto>;
