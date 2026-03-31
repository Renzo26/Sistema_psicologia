using MediatR;
using PsicoFinance.Application.Features.Fechamentos.DTOs;

namespace PsicoFinance.Application.Features.Fechamentos.Commands.RealizarFechamentoMensal;

public record RealizarFechamentoMensalCommand(
    string MesReferencia,
    string? Observacao) : IRequest<FechamentoDto>;
