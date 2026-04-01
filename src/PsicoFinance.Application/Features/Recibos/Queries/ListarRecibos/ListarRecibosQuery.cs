using MediatR;
using PsicoFinance.Application.Features.Recibos.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Recibos.Queries.ListarRecibos;

public record ListarRecibosQuery(
    Guid? PacienteId,
    DateOnly? DataInicio,
    DateOnly? DataFim,
    StatusRecibo? Status) : IRequest<List<ReciboDto>>;
