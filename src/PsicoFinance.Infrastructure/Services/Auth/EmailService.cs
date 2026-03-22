using Microsoft.Extensions.Logging;
using PsicoFinance.Application.Common.Interfaces;

namespace PsicoFinance.Infrastructure.Services.Auth;

public class EmailService : IEmailService
{
    private readonly ILogger<EmailService> _logger;

    public EmailService(ILogger<EmailService> logger)
    {
        _logger = logger;
    }

    public Task SendPasswordResetEmailAsync(string email, string resetToken, CancellationToken cancellationToken = default)
    {
        // TODO: Implementar envio real de email (SMTP, SendGrid, etc.)
        // Por enquanto, loga o token para desenvolvimento
        _logger.LogInformation(
            "Password reset requested for {Email}. Token: {Token}",
            email,
            resetToken);

        return Task.CompletedTask;
    }
}
