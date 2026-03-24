using FluentValidation;

namespace PsicoFinance.Application.Features.Onboarding.Commands;

public class OnboardingCommandValidator : AbstractValidator<OnboardingCommand>
{
    public OnboardingCommandValidator()
    {
        RuleFor(x => x.NomeClinica)
            .NotEmpty().WithMessage("Nome da clínica é obrigatório")
            .MaximumLength(150);

        RuleFor(x => x.EmailClinica)
            .NotEmpty().WithMessage("Email da clínica é obrigatório")
            .EmailAddress().WithMessage("Email da clínica inválido");

        RuleFor(x => x.Cnpj)
            .MaximumLength(18)
            .When(x => !string.IsNullOrEmpty(x.Cnpj));

        RuleFor(x => x.NomeAdmin)
            .NotEmpty().WithMessage("Nome do administrador é obrigatório")
            .MaximumLength(150);

        RuleFor(x => x.EmailAdmin)
            .NotEmpty().WithMessage("Email do administrador é obrigatório")
            .EmailAddress().WithMessage("Email do administrador inválido");

        RuleFor(x => x.SenhaAdmin)
            .NotEmpty().WithMessage("Senha é obrigatória")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres")
            .Matches("[A-Z]").WithMessage("Senha deve conter pelo menos uma letra maiúscula")
            .Matches("[a-z]").WithMessage("Senha deve conter pelo menos uma letra minúscula")
            .Matches("[0-9]").WithMessage("Senha deve conter pelo menos um número");
    }
}
