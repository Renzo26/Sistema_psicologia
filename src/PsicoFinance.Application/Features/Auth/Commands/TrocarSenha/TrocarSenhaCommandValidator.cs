using FluentValidation;

namespace PsicoFinance.Application.Features.Auth.Commands.TrocarSenha;

public class TrocarSenhaCommandValidator : AbstractValidator<TrocarSenhaCommand>
{
    public TrocarSenhaCommandValidator()
    {
        RuleFor(x => x.UsuarioId)
            .NotEmpty().WithMessage("Usuário é obrigatório.");

        RuleFor(x => x.SenhaAtual)
            .NotEmpty().WithMessage("Senha atual é obrigatória.");

        RuleFor(x => x.NovaSenha)
            .NotEmpty().WithMessage("Nova senha é obrigatória.")
            .MinimumLength(8).WithMessage("Nova senha deve ter no mínimo 8 caracteres.")
            .Matches("[A-Z]").WithMessage("Nova senha deve conter pelo menos uma letra maiúscula.")
            .Matches("[a-z]").WithMessage("Nova senha deve conter pelo menos uma letra minúscula.")
            .Matches("[0-9]").WithMessage("Nova senha deve conter pelo menos um número.");
    }
}
