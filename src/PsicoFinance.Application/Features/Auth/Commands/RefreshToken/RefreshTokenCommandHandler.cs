using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Auth.DTOs;

namespace PsicoFinance.Application.Features.Auth.Commands.RefreshToken;

public class RefreshTokenCommandHandler : IRequestHandler<RefreshTokenCommand, AuthResponseDto>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public RefreshTokenCommandHandler(
        IAppDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> Handle(RefreshTokenCommand request, CancellationToken cancellationToken)
    {
        var tokens = await _context.RefreshTokens
            .Include(rt => rt.Usuario)
                .ThenInclude(u => u.Clinica)
            .Where(rt => !rt.Revogado && rt.ExpiraEm > DateTimeOffset.UtcNow)
            .ToListAsync(cancellationToken);

        var storedToken = tokens
            .FirstOrDefault(rt => _passwordHasher.Verify(request.RefreshToken, rt.TokenHash));

        if (storedToken is null)
            throw new UnauthorizedAccessException("Refresh token inválido ou expirado.");

        if (!storedToken.Usuario.Ativo)
            throw new UnauthorizedAccessException("Usuário inativo.");

        // Revogar token atual (rotation)
        storedToken.Revogado = true;
        storedToken.RevogadoEm = DateTimeOffset.UtcNow;

        // Gerar novo refresh token
        var newRefreshTokenValue = _jwtService.GenerateRefreshToken();
        var newRefreshToken = new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UsuarioId = storedToken.UsuarioId,
            ClinicaId = storedToken.ClinicaId,
            TokenHash = _passwordHasher.Hash(newRefreshTokenValue),
            ExpiraEm = DateTimeOffset.UtcNow.AddDays(7),
            IpOrigem = request.IpOrigem,
            UserAgent = request.UserAgent
        };

        _context.RefreshTokens.Add(newRefreshToken);

        var usuario = storedToken.Usuario;
        var accessToken = _jwtService.GenerateAccessToken(usuario);

        await _context.SaveChangesAsync(cancellationToken);

        return new AuthResponseDto(
            UsuarioId: usuario.Id,
            Nome: usuario.Nome,
            Email: usuario.Email,
            Role: usuario.Role,
            ClinicaId: usuario.ClinicaId,
            AccessToken: accessToken
        );
    }
}
