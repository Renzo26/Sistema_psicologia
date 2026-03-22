using FluentValidation;

namespace PsicoFinance.Application.Features.Auth.Commands.Register;

public class RegisterCommandValidator : AbstractValidator<RegisterCommand>
{
    public RegisterCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(200).WithMessage("Nome deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.");

        RuleFor(x => x.Senha)
            .NotEmpty().WithMessage("Senha é obrigatória.")
            .MinimumLength(8).WithMessage("Senha deve ter no mínimo 8 caracteres.")
            .Matches("[A-Z]").WithMessage("Senha deve conter pelo menos uma letra maiúscula.")
            .Matches("[a-z]").WithMessage("Senha deve conter pelo menos uma letra minúscula.")
            .Matches("[0-9]").WithMessage("Senha deve conter pelo menos um número.");

        RuleFor(x => x.Role)
            .IsInEnum().WithMessage("Role inválida.");

        RuleFor(x => x.ClinicaId)
            .NotEmpty().WithMessage("Clínica é obrigatória.");
    }
}
