namespace PsicoFinance.Application.Features.Contratos.DTOs;

public record ContratoDto(
    Guid Id,
    Guid PacienteId,
    string PacienteNome,
    Guid PsicologoId,
    string PsicologoNome,
    decimal ValorSessao,
    string FormaPagamento,
    string Frequencia,
    string DiaSemanaSessao,
    TimeOnly HorarioSessao,
    int DuracaoMinutos,
    bool CobraFaltaInjustificada,
    bool CobraFaltaJustificada,
    DateOnly DataInicio,
    DateOnly? DataFim,
    string Status,
    string? MotivoEncerramento,
    Guid? PlanoContaId,
    string? Observacoes,
    DateTimeOffset CriadoEm);

public record ContratoResumoDto(
    Guid Id,
    string PacienteNome,
    string PsicologoNome,
    decimal ValorSessao,
    string Frequencia,
    string DiaSemanaSessao,
    TimeOnly HorarioSessao,
    string Status);
