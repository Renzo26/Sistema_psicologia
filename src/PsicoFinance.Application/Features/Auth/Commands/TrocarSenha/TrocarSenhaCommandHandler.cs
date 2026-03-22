using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;

namespace PsicoFinance.Application.Features.Auth.Commands.TrocarSenha;

public class TrocarSenhaCommandHandler : IRequestHandler<TrocarSenhaCommand, Unit>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public TrocarSenhaCommandHandler(IAppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Unit> Handle(TrocarSenhaCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Id == request.UsuarioId && u.Ativo, cancellationToken)
            ?? throw new KeyNotFoundException("Usuário não encontrado.");

        if (!_passwordHasher.Verify(request.SenhaAtual, usuario.SenhaHash))
            throw new UnauthorizedAccessException("Senha atual incorreta.");

        usuario.SenhaHash = _passwordHasher.Hash(request.NovaSenha);
        usuario.AtualizadoEm = DateTimeOffset.UtcNow;

        // Revogar todos os refresh tokens (force re-login em outros dispositivos)
        var tokens = await _context.RefreshTokens
            .Where(rt => rt.UsuarioId == usuario.Id && !rt.Revogado)
            .ToListAsync(cancellationToken);

        foreach (var token in tokens)
        {
            token.Revogado = true;
            token.RevogadoEm = DateTimeOffset.UtcNow;
        }

        await _context.SaveChangesAsync(cancellationToken);
        return Unit.Value;
    }
}
