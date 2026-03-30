namespace PsicoFinance.Application.Features.Sessoes.DTOs;

public record SessaoDto(
    Guid Id,
    Guid ContratoId,
    Guid PacienteId,
    string PacienteNome,
    Guid PsicologoId,
    string PsicologoNome,
    DateOnly Data,
    TimeOnly HorarioInicio,
    int DuracaoMinutos,
    string Status,
    string? Observacoes,
    string? MotivoFalta,
    DateTimeOffset CriadoEm);

public record SessaoResumoDto(
    Guid Id,
    Guid ContratoId,
    string PacienteNome,
    string PsicologoNome,
    DateOnly Data,
    TimeOnly HorarioInicio,
    int DuracaoMinutos,
    string Status);
