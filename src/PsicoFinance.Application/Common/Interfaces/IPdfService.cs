namespace PsicoFinance.Application.Common.Interfaces;

/// <summary>
/// Dados necessários para gerar um recibo PDF.
/// </summary>
public record ReciboData(
    string NumeroRecibo,
    // Clínica
    string ClinicaNome,
    string? ClinicaCnpj,
    string? ClinicaTelefone,
    string? ClinicaEmail,
    string? ClinicaEndereco,
    // Paciente
    string PacienteNome,
    string? PacienteCpf,
    // Psicólogo
    string PsicologoNome,
    string PsicologoCrp,
    // Sessão
    DateOnly DataSessao,
    TimeOnly HorarioSessao,
    int DuracaoMinutos,
    // Financeiro
    decimal Valor,
    string FormaPagamento,
    DateOnly DataEmissao
);

/// <summary>
/// Dados de uma sessão para o relatório mensal.
/// </summary>
public record SessaoRelatorioItem(
    DateOnly Data,
    TimeOnly Horario,
    string PacienteNome,
    string Status,
    decimal Valor
);

/// <summary>
/// Dados necessários para gerar um relatório mensal de psicólogo.
/// </summary>
public record RelatorioMensalData(
    // Clínica
    string ClinicaNome,
    string? ClinicaCnpj,
    // Psicólogo
    string PsicologoNome,
    string PsicologoCrp,
    // Período
    string Competencia,
    // Sessões
    List<SessaoRelatorioItem> Sessoes,
    int TotalRealizadas,
    int TotalFaltas,
    int TotalCanceladas,
    // Financeiro
    decimal ReceitaTotal,
    decimal ValorRepasse,
    string TipoRepasse,
    decimal PercentualOuValorRepasse
);

public interface IPdfService
{
    /// <summary>
    /// Gera um recibo em PDF e retorna os bytes.
    /// </summary>
    byte[] GerarRecibo(ReciboData data);

    /// <summary>
    /// Gera um relatório mensal de psicólogo em PDF e retorna os bytes.
    /// </summary>
    byte[] GerarRelatorioMensalPsicologo(RelatorioMensalData data);
}
