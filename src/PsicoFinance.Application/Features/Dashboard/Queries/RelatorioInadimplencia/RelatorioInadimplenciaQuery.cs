using MediatR;
using PsicoFinance.Application.Features.Dashboard.DTOs;

namespace PsicoFinance.Application.Features.Dashboard.Queries.RelatorioInadimplencia;

public record RelatorioInadimplenciaQuery(
    DateOnly? DataBase) : IRequest<RelatorioInadimplenciaDto>;
