namespace PsicoFinance.Application.Features.Pacientes.DTOs;

public record PacienteDto(
    Guid Id,
    string Nome,
    string? Cpf,
    string? Email,
    string? Telefone,
    DateOnly? DataNascimento,
    string? ResponsavelNome,
    string? ResponsavelTelefone,
    string? Observacoes,
    bool Ativo,
    DateTimeOffset CriadoEm);

public record PacienteResumoDto(
    Guid Id,
    string Nome,
    string? Cpf,
    string? Telefone,
    bool Ativo);
