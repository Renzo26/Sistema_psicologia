using System.Security.Cryptography;
using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;

namespace PsicoFinance.Application.Features.Auth.Commands.RecuperarSenha;

public class SolicitarRecuperacaoSenhaCommandHandler : IRequestHandler<SolicitarRecuperacaoSenhaCommand, Unit>
{
    private readonly IAppDbContext _context;
    private readonly IEmailService _emailService;
    private readonly IPasswordHasher _passwordHasher;

    public SolicitarRecuperacaoSenhaCommandHandler(
        IAppDbContext context,
        IEmailService emailService,
        IPasswordHasher passwordHasher)
    {
        _context = context;
        _emailService = emailService;
        _passwordHasher = passwordHasher;
    }

    public async Task<Unit> Handle(SolicitarRecuperacaoSenhaCommand request, CancellationToken cancellationToken)
    {
        var usuario = await _context.Usuarios
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.Ativo, cancellationToken);

        // Sempre retorna sucesso para não revelar se o email existe
        if (usuario is null)
            return Unit.Value;

        // Gerar token de reset (válido por 1 hora)
        var resetToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));

        // Salvar token hash como refresh token com marcador especial
        var token = new Domain.Entities.RefreshToken
        {
            Id = Guid.NewGuid(),
            UsuarioId = usuario.Id,
            ClinicaId = usuario.ClinicaId,
            TokenHash = _passwordHasher.Hash(resetToken),
            ExpiraEm = DateTimeOffset.UtcNow.AddHours(1),
            UserAgent = "PASSWORD_RESET"
        };

        _context.RefreshTokens.Add(token);
        await _context.SaveChangesAsync(cancellationToken);

        await _emailService.SendPasswordResetEmailAsync(usuario.Email, resetToken, cancellationToken);

        return Unit.Value;
    }
}
