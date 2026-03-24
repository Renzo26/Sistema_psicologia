using MediatR;
using Microsoft.AspNetCore.Mvc;
using PsicoFinance.Application.Features.Onboarding.Commands;

namespace PsicoFinance.Api.Controllers;

[ApiController]
[Route("api/[controller]")]
public class OnboardingController : ControllerBase
{
    private readonly IMediator _mediator;

    public OnboardingController(IMediator mediator)
    {
        _mediator = mediator;
    }

    /// <summary>
    /// Cria uma nova clínica + usuário admin + schema PostgreSQL.
    /// Endpoint público (não requer autenticação).
    /// </summary>
    [HttpPost]
    public async Task<IActionResult> Onboarding([FromBody] OnboardingRequest request)
    {
        var command = new OnboardingCommand(
            NomeClinica: request.NomeClinica,
            Cnpj: request.Cnpj,
            EmailClinica: request.EmailClinica,
            Telefone: request.Telefone,
            NomeAdmin: request.NomeAdmin,
            EmailAdmin: request.EmailAdmin,
            SenhaAdmin: request.SenhaAdmin,
            IpOrigem: HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent: Request.Headers.UserAgent.ToString());

        var result = await _mediator.Send(command);

        return Created($"/api/clinicas/{result.ClinicaId}", result);
    }
}

public record OnboardingRequest(
    string NomeClinica,
    string? Cnpj,
    string EmailClinica,
    string? Telefone,
    string NomeAdmin,
    string EmailAdmin,
    string SenhaAdmin);
