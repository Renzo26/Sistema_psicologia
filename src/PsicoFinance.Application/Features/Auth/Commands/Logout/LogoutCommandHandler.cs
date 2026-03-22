using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;

namespace PsicoFinance.Application.Features.Auth.Commands.Logout;

public class LogoutCommandHandler : IRequestHandler<LogoutCommand, Unit>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public LogoutCommandHandler(IAppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<Unit> Handle(LogoutCommand request, CancellationToken cancellationToken)
    {
        var tokens = await _context.RefreshTokens
            .Where(rt => !rt.Revogado)
            .ToListAsync(cancellationToken);

        var storedToken = tokens
            .FirstOrDefault(rt => _passwordHasher.Verify(request.RefreshToken, rt.TokenHash));

        if (storedToken is not null)
        {
            storedToken.Revogado = true;
            storedToken.RevogadoEm = DateTimeOffset.UtcNow;
            await _context.SaveChangesAsync(cancellationToken);
        }

        return Unit.Value;
    }
}
