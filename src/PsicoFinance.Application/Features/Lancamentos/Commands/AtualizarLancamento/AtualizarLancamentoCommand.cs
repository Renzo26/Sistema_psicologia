using MediatR;
using PsicoFinance.Application.Features.Lancamentos.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Lancamentos.Commands.AtualizarLancamento;

public record AtualizarLancamentoCommand(
    Guid Id,
    string Descricao,
    decimal Valor,
    TipoLancamento Tipo,
    DateOnly DataVencimento,
    string Competencia,
    Guid PlanoContaId,
    string? Observacao) : IRequest<LancamentoDto>;
