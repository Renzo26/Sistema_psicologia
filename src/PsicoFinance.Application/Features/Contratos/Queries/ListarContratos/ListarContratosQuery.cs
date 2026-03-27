using MediatR;
using PsicoFinance.Application.Features.Contratos.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Contratos.Queries.ListarContratos;

public record ListarContratosQuery(
    string? Busca = null,
    StatusContrato? Status = null,
    Guid? PsicologoId = null,
    Guid? PacienteId = null) : IRequest<List<ContratoResumoDto>>;
