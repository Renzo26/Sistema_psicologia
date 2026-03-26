using MediatR;
using PsicoFinance.Application.Features.Pacientes.DTOs;

namespace PsicoFinance.Application.Features.Pacientes.Queries.ObterPaciente;

public record ObterPacienteQuery(Guid Id) : IRequest<PacienteDto>;
