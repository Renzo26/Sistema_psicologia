using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Lancamentos.DTOs;

public record LancamentoDto(
    Guid Id,
    string Descricao,
    decimal Valor,
    TipoLancamento Tipo,
    StatusLancamento Status,
    DateOnly DataVencimento,
    DateOnly? DataPagamento,
    string Competencia,
    Guid? SessaoId,
    Guid PlanoContaId,
    string PlanoContaNome,
    string? Observacao,
    DateTimeOffset CriadoEm);

public record FluxoCaixaDto(
    string Competencia,
    decimal TotalReceitasPrevisto,
    decimal TotalReceitasConfirmado,
    decimal TotalDespesasPrevisto,
    decimal TotalDespesasConfirmado,
    decimal SaldoPrevisto,
    decimal SaldoRealizado,
    List<FluxoCaixaDiaDto> Dias);

public record FluxoCaixaDiaDto(
    DateOnly Data,
    decimal ReceitasPrevisto,
    decimal ReceitasConfirmado,
    decimal DespesasPrevisto,
    decimal DespesasConfirmado);
