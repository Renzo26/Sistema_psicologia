using MediatR;
using PsicoFinance.Application.Features.Dashboard.DTOs;

namespace PsicoFinance.Application.Features.Dashboard.Queries.RelatorioSessoesPeriodo;

public record RelatorioSessoesPeriodoQuery(
    DateOnly DataInicio,
    DateOnly DataFim) : IRequest<RelatorioSessoesPeriodoDto>;
