using FluentValidation;

namespace PsicoFinance.Application.Features.Auth.Commands.RecuperarSenha;

public class SolicitarRecuperacaoSenhaCommandValidator : AbstractValidator<SolicitarRecuperacaoSenhaCommand>
{
    public SolicitarRecuperacaoSenhaCommandValidator()
    {
        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.");
    }
}

public class RedefinirSenhaCommandValidator : AbstractValidator<RedefinirSenhaCommand>
{
    public RedefinirSenhaCommandValidator()
    {
        RuleFor(x => x.Token)
            .NotEmpty().WithMessage("Token é obrigatório.");

        RuleFor(x => x.NovaSenha)
            .NotEmpty().WithMessage("Nova senha é obrigatória.")
            .MinimumLength(8).WithMessage("Nova senha deve ter no mínimo 8 caracteres.")
            .Matches("[A-Z]").WithMessage("Nova senha deve conter pelo menos uma letra maiúscula.")
            .Matches("[a-z]").WithMessage("Nova senha deve conter pelo menos uma letra minúscula.")
            .Matches("[0-9]").WithMessage("Nova senha deve conter pelo menos um número.");
    }
}
