using MediatR;
using PsicoFinance.Application.Features.Sessoes.DTOs;

namespace PsicoFinance.Application.Features.Sessoes.Commands.AtualizarSessao;

public record AtualizarSessaoCommand(
    Guid Id,
    DateOnly Data,
    TimeOnly HorarioInicio,
    int DuracaoMinutos,
    string? Observacoes) : IRequest<SessaoDto>;
