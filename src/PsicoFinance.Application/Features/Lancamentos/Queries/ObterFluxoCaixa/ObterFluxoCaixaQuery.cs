using MediatR;
using PsicoFinance.Application.Features.Lancamentos.DTOs;

namespace PsicoFinance.Application.Features.Lancamentos.Queries.ObterFluxoCaixa;

public record ObterFluxoCaixaQuery(string Competencia) : IRequest<FluxoCaixaDto>;
