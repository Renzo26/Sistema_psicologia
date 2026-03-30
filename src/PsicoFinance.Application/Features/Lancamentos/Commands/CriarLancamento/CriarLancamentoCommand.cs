using MediatR;
using PsicoFinance.Application.Features.Lancamentos.DTOs;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Lancamentos.Commands.CriarLancamento;

public record CriarLancamentoCommand(
    string Descricao,
    decimal Valor,
    TipoLancamento Tipo,
    DateOnly DataVencimento,
    string Competencia,
    Guid PlanoContaId,
    Guid? SessaoId,
    string? Observacao) : IRequest<LancamentoDto>;
