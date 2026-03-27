using MediatR;
using PsicoFinance.Application.Features.Contratos.DTOs;

namespace PsicoFinance.Application.Features.Contratos.Queries.ObterContrato;

public record ObterContratoQuery(Guid Id) : IRequest<ContratoDto>;
