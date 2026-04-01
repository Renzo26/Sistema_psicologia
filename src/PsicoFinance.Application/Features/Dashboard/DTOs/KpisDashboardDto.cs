namespace PsicoFinance.Application.Features.Dashboard.DTOs;

public record KpisDashboardDto(
    string Competencia,
    // Financeiro
    decimal ReceitaPrevista,
    decimal ReceitaRealizada,
    decimal DespesaPrevista,
    decimal DespesaRealizada,
    decimal SaldoPrevisto,
    decimal SaldoRealizado,
    decimal TicketMedio,
    decimal TaxaInadimplencia,
    // Sessões
    int TotalSessoesAgendadas,
    int TotalSessoesRealizadas,
    int TotalSessoesFalta,
    int TotalSessoesFaltaJustificada,
    int TotalSessoesCanceladas,
    decimal TaxaAbsenteismo,
    decimal TaxaOcupacao,
    // Ranking
    List<RankingPsicologoDto> RankingPsicologos,
    // Inadimplência
    List<InadimplentePacienteDto> PacientesInadimplentes);

public record RankingPsicologoDto(
    Guid PsicologoId,
    string Nome,
    int TotalSessoes,
    int SessoesRealizadas,
    decimal ReceitaGerada,
    decimal TaxaAbsenteismo);

public record InadimplentePacienteDto(
    Guid PacienteId,
    string Nome,
    int TotalLancamentosVencidos,
    decimal ValorTotal,
    DateOnly VencimentoMaisAntigo);
