using MediatR;
using PsicoFinance.Application.Features.Lancamentos.DTOs;

namespace PsicoFinance.Application.Features.Lancamentos.Commands.ConfirmarPagamento;

public record ConfirmarPagamentoCommand(Guid Id, DateOnly DataPagamento) : IRequest<LancamentoDto>;
