using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;

namespace PsicoFinance.Application.Features.Auth.Commands.RecuperarSenha;

public class RedefinirSenhaCommandHandler : IRequestHandler<RedefinirSenhaCommand, Unit>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RedefinirSenhaCommandHandler(IAppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Unit> Handle(RedefinirSenhaCommand request, CancellationToken cancellationToken)
    {
        var tokens = await _context.RefreshTokens
            .Include(rt => rt.Usuario)
            .Where(rt => !rt.Revogado
                && rt.ExpiraEm > DateTimeOffset.UtcNow
                && rt.UserAgent == "PASSWORD_RESET")
            .ToListAsync(cancellationToken);

        var storedToken = tokens
            .FirstOrDefault(rt => _passwordHasher.Verify(request.Token, rt.TokenHash));

        if (storedToken is null)
            throw new ArgumentException("Token de recuperação inválido ou expirado.");

        var usuario = storedToken.Usuario;
        usuario.SenhaHash = _passwordHasher.Hash(request.NovaSenha);
        usuario.AtualizadoEm = DateTimeOffset.UtcNow;

        // Revogar token de reset
        storedToken.Revogado = true;
        storedToken.RevogadoEm = DateTimeOffset.UtcNow;

        // Revogar todos os refresh tokens do usuário
        var allTokens = await _context.RefreshTokens
            .Where(rt => rt.UsuarioId == usuario.Id && !rt.Revogado)
            .ToListAsync(cancellationToken);

        foreach (var token in allTokens)
        {
            token.Revogado = true;
            token.RevogadoEm = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
