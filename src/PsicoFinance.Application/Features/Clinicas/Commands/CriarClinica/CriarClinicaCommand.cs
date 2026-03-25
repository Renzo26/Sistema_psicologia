using MediatR;
using PsicoFinance.Application.Features.Clinicas.DTOs;

namespace PsicoFinance.Application.Features.Clinicas.Commands.CriarClinica;

public record CriarClinicaCommand(
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
    string? Estado) : IRequest<ClinicaDto>;
