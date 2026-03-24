using MediatR;
using PsicoFinance.Application.Features.Clinicas.DTOs;

namespace PsicoFinance.Application.Features.Clinicas.Commands.AtualizarClinica;

public record AtualizarClinicaCommand(
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
    string Timezone) : IRequest<ClinicaDto>;
