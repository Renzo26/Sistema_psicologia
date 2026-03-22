using System.Security.Claims;
using MediatR;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.RateLimiting;
using PsicoFinance.Application.Features.Auth.Commands.Login;
using PsicoFinance.Application.Features.Auth.Commands.Logout;
using PsicoFinance.Application.Features.Auth.Commands.RecuperarSenha;
using PsicoFinance.Application.Features.Auth.Commands.RefreshToken;
using PsicoFinance.Application.Features.Auth.Commands.Register;
using PsicoFinance.Application.Features.Auth.Commands.TrocarSenha;
using PsicoFinance.Application.Features.Auth.DTOs;

namespace PsicoFinance.Api.Controllers;

[ApiController]
[Route("api/auth")]
[EnableRateLimiting("public")]
public class AuthController : ControllerBase
{
    private readonly ISender _mediator;

    public AuthController(ISender mediator) => _mediator = mediator;

    /// <summary>
    /// Login — retorna access token + refresh token (cookie HttpOnly)
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request, CancellationToken ct)
    {
        var command = new LoginCommand(
            Email: request.Email,
            Senha: request.Senha,
            IpOrigem: HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent: Request.Headers.UserAgent.ToString()
        );

        var result = await _mediator.Send(command, ct);

        // Refresh token vai como cookie HttpOnly (não acessível via JS)
        SetRefreshTokenCookie(result.AccessToken);

        return Ok(result);
    }

    /// <summary>
    /// Registro de novo usuário (requer Admin ou Gerente)
    /// </summary>
    [HttpPost("register")]
    [Authorize(Roles = "Admin,Gerente")]
    [EnableRateLimiting("authenticated")]
    [ProducesResponseType(typeof(UsuarioDto), 201)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request, CancellationToken ct)
    {
        var clinicaId = GetClinicaIdFromClaims();

        var command = new RegisterCommand(
            Nome: request.Nome,
            Email: request.Email,
            Senha: request.Senha,
            Role: request.Role,
            ClinicaId: clinicaId,
            PsicologoId: request.PsicologoId
        );

        var result = await _mediator.Send(command, ct);
        return Created($"api/usuarios/{result.Id}", result);
    }

    /// <summary>
    /// Refresh token — gera novo access token + rotaciona refresh token
    /// </summary>
    [HttpPost("refresh")]
    [ProducesResponseType(typeof(AuthResponseDto), 200)]
    [ProducesResponseType(401)]
    public async Task<IActionResult> Refresh(CancellationToken ct)
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (string.IsNullOrEmpty(refreshToken))
            return Unauthorized(new { message = "Refresh token não encontrado." });

        var command = new RefreshTokenCommand(
            RefreshToken: refreshToken,
            IpOrigem: HttpContext.Connection.RemoteIpAddress?.ToString(),
            UserAgent: Request.Headers.UserAgent.ToString()
        );

        var result = await _mediator.Send(command, ct);
        SetRefreshTokenCookie(refreshToken);

        return Ok(result);
    }

    /// <summary>
    /// Logout — revoga o refresh token
    /// </summary>
    [HttpPost("logout")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> Logout(CancellationToken ct)
    {
        var refreshToken = Request.Cookies["refresh_token"];
        if (!string.IsNullOrEmpty(refreshToken))
        {
            await _mediator.Send(new LogoutCommand(refreshToken), ct);
            Response.Cookies.Delete("refresh_token");
        }

        return NoContent();
    }

    /// <summary>
    /// Trocar senha do usuário autenticado
    /// </summary>
    [HttpPost("trocar-senha")]
    [Authorize]
    [EnableRateLimiting("authenticated")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> TrocarSenha([FromBody] TrocarSenhaRequest request, CancellationToken ct)
    {
        var usuarioId = GetUsuarioIdFromClaims();

        var command = new TrocarSenhaCommand(
            UsuarioId: usuarioId,
            SenhaAtual: request.SenhaAtual,
            NovaSenha: request.NovaSenha
        );

        await _mediator.Send(command, ct);
        Response.Cookies.Delete("refresh_token");

        return NoContent();
    }

    /// <summary>
    /// Solicitar recuperação de senha (envia email)
    /// </summary>
    [HttpPost("recuperar-senha")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> SolicitarRecuperacao([FromBody] RecuperarSenhaRequest request, CancellationToken ct)
    {
        await _mediator.Send(new SolicitarRecuperacaoSenhaCommand(request.Email), ct);
        return NoContent();
    }

    /// <summary>
    /// Redefinir senha com token de recuperação
    /// </summary>
    [HttpPost("redefinir-senha")]
    [ProducesResponseType(204)]
    public async Task<IActionResult> RedefinirSenha([FromBody] RedefinirSenhaRequest request, CancellationToken ct)
    {
        await _mediator.Send(new RedefinirSenhaCommand(request.Token, request.NovaSenha), ct);
        return NoContent();
    }

    // ── Helpers ──────────────────────────────────────────────────

    private void SetRefreshTokenCookie(string token)
    {
        var cookieOptions = new CookieOptions
        {
            HttpOnly = true,
            Secure = true,
            SameSite = SameSiteMode.Strict,
            Expires = DateTimeOffset.UtcNow.AddDays(7)
        };

        Response.Cookies.Append("refresh_token", token, cookieOptions);
    }

    private Guid GetUsuarioIdFromClaims()
    {
        var claim = User.FindFirstValue("sub")
            ?? throw new UnauthorizedAccessException("Token inválido.");
        return Guid.Parse(claim);
    }

    private Guid GetClinicaIdFromClaims()
    {
        var claim = User.FindFirstValue("clinica_id")
            ?? throw new UnauthorizedAccessException("Token inválido.");
        return Guid.Parse(claim);
    }
}

// ── Request DTOs (entrada do controller) ────────────────────────

public record LoginRequest(string Email, string Senha);

public record RegisterRequest(
    string Nome,
    string Email,
    string Senha,
    PsicoFinance.Domain.Enums.RoleUsuario Role,
    Guid? PsicologoId
);

public record TrocarSenhaRequest(string SenhaAtual, string NovaSenha);
public record RecuperarSenhaRequest(string Email);
public record RedefinirSenhaRequest(string Token, string NovaSenha);
