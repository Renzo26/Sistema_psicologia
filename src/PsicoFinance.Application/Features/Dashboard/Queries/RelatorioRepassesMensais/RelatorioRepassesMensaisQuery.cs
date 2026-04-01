using MediatR;
using PsicoFinance.Application.Features.Dashboard.DTOs;

namespace PsicoFinance.Application.Features.Dashboard.Queries.RelatorioRepassesMensais;

public record RelatorioRepassesMensaisQuery(
    string? CompetenciaInicio,
    string? CompetenciaFim) : IRequest<RelatorioRepassesMensaisDto>;
