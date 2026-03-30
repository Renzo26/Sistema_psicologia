using MediatR;
using PsicoFinance.Application.Features.Repasses.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Repasses.Queries.ListarRepasses;

public record ListarRepassesQuery(
    string? MesReferencia = null,
    Guid? PsicologoId = null,
    StatusRepasse? Status = null) : IRequest<List<RepasseDto>>;
