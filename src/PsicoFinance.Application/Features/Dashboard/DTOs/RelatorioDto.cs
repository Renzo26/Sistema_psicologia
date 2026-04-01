namespace PsicoFinance.Application.Features.Dashboard.DTOs;

public record RelatorioFluxoCaixaMensalDto(
    string Competencia,
    decimal ReceitasPrevisto,
    decimal ReceitasConfirmado,
    decimal DespesasPrevisto,
    decimal DespesasConfirmado,
    decimal SaldoPrevisto,
    decimal SaldoRealizado);

public record RelatorioSessoesPeriodoDto(
    DateOnly DataInicio,
    DateOnly DataFim,
    int TotalAgendadas,
    int TotalRealizadas,
    int TotalFaltas,
    int TotalCanceladas,
    decimal TaxaAbsenteismo,
    List<SessoesPorPsicologoDto> PorPsicologo);

public record SessoesPorPsicologoDto(
    Guid PsicologoId,
    string PsicologoNome,
    int Total,
    int Realizadas,
    int Faltas,
    int Canceladas);

public record RelatorioRepassesMensaisDto(
    List<RepasseMensalItemDto> Itens);

public record RepasseMensalItemDto(
    string MesReferencia,
    Guid PsicologoId,
    string PsicologoNome,
    decimal ValorRepasse,
    string Status);

public record RelatorioInadimplenciaDto(
    DateOnly DataBase,
    int TotalPacientes,
    int TotalLancamentos,
    decimal ValorTotal,
    List<InadimplenciaItemDto> Itens);

public record InadimplenciaItemDto(
    Guid PacienteId,
    string PacienteNome,
    Guid LancamentoId,
    string Descricao,
    decimal Valor,
    DateOnly Vencimento,
    int DiasAtraso);
