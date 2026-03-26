using MediatR;
using PsicoFinance.Application.Features.Pacientes.DTOs;

namespace PsicoFinance.Application.Features.Pacientes.Queries.ListarPacientes;

public record ListarPacientesQuery(
    string? Busca = null,
    bool? ApenasAtivos = true) : IRequest<List<PacienteResumoDto>>;
