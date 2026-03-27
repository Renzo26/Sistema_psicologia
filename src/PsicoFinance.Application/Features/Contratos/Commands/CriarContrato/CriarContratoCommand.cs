using MediatR;
using PsicoFinance.Application.Features.Contratos.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Contratos.Commands.CriarContrato;

public record CriarContratoCommand(
    Guid PacienteId,
    Guid PsicologoId,
    decimal ValorSessao,
    FormaPagamento FormaPagamento,
    FrequenciaContrato Frequencia,
    DiaSemana DiaSemanaSessao,
    TimeOnly HorarioSessao,
    int DuracaoMinutos,
    bool CobraFaltaInjustificada,
    bool CobraFaltaJustificada,
    DateOnly DataInicio,
    DateOnly? DataFim,
    Guid? PlanoContaId,
    string? Observacoes) : IRequest<ContratoDto>;
