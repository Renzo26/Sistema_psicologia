namespace PsicoFinance.Application.Features.Clinicas.DTOs;

public record ClinicaDto(
    Guid Id,
    string Nome,
    string? Cnpj,
    string Email,
    string? Telefone,
    string? Cep,
    string? Logradouro,
    string? Numero,
    string? Complemento,
    string? Bairro,
    string? Cidade,
    string? Estado,
    TimeOnly HorarioEnvioAlerta,
    string? WebhookN8nUrl,
    string Timezone,
    bool Ativo,
    DateTimeOffset CriadoEm);
