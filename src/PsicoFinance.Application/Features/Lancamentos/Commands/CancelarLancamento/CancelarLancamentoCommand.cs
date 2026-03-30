using MediatR;

namespace PsicoFinance.Application.Features.Lancamentos.Commands.CancelarLancamento;

public record CancelarLancamentoCommand(Guid Id, string? Motivo) : IRequest;
