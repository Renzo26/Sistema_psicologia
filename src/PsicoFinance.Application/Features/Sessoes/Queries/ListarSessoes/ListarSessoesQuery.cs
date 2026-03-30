using MediatR;
using PsicoFinance.Application.Features.Sessoes.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Sessoes.Queries.ListarSessoes;

public record ListarSessoesQuery(
    DateOnly? DataInicio,
    DateOnly? DataFim,
    Guid? PsicologoId,
    Guid? PacienteId,
    Guid? ContratoId,
    StatusSessao? Status) : IRequest<List<SessaoResumoDto>>;
