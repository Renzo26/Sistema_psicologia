using MediatR;
using PsicoFinance.Application.Features.RelatoriosBI.DTOs;

namespace PsicoFinance.Application.Features.RelatoriosBI.Queries.ObterRelatorioPersonalizado;

public record ObterRelatorioPersonalizadoQuery(Guid Id) : IRequest<RelatorioPersonalizadoDto>;
