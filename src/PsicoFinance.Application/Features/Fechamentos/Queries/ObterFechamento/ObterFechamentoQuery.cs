using MediatR;
using PsicoFinance.Application.Features.Fechamentos.DTOs;

namespace PsicoFinance.Application.Features.Fechamentos.Queries.ObterFechamento;

public record ObterFechamentoQuery(string MesReferencia) : IRequest<FechamentoDto>;
