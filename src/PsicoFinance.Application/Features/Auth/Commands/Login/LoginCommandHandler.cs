using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Auth.DTOs;

namespace PsicoFinance.Application.Features.Auth.Commands.Login;

public class LoginCommandHandler : IRequestHandler<LoginCommand, AuthResponseDto>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;

    public LoginCommandHandler(
        IAppDbContext context,
        IPasswordHasher passwordHasher,
        IJwtService jwtService)
    {
        _context = context;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
    }

    public async Task<AuthResponseDto> Handle(LoginCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .Include(u => u.Clinica)
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Ativo, cancellationToken);

        if (usuario is null || !_passwordHasher.Verify(request.Senha, usuario.SenhaHash))
            throw new UnauthorizedAccessException("Email ou senha inválidos.");

        if (!usuario.Clinica.Ativo)
            throw new UnauthorizedAccessException("Clínica inativa. Entre em contato com o suporte.");

        var accessToken = _jwtService.GenerateAccessToken(usuario);
        var refreshTokenValue = _jwtService.GenerateRefreshToken();

        var refreshToken = new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            ClinicaId = usuario.ClinicaId,
            TokenHash = _passwordHasher.Hash(refreshTokenValue),
            ExpiraEm = DateTimeOffset.UtcNow.AddDays(7),
            IpOrigem = request.IpOrigem,
            UserAgent = request.UserAgent
        };

        _context.RefreshTokens.Add(refreshToken);

        usuario.UltimoAcessoEm = DateTimeOffset.UtcNow;
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
