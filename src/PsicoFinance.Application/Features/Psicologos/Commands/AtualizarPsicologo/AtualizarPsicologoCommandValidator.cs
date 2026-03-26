using FluentValidation;

namespace PsicoFinance.Application.Features.Psicologos.Commands.AtualizarPsicologo;

public class AtualizarPsicologoCommandValidator : AbstractValidator<AtualizarPsicologoCommand>
{
    public AtualizarPsicologoCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id é obrigatório.");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(150);

        RuleFor(x => x.Crp)
            .NotEmpty().WithMessage("CRP é obrigatório.")
            .MaximumLength(20);

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email inválido.")
            .MaximumLength(254)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Telefone).MaximumLength(20).When(x => !string.IsNullOrWhiteSpace(x.Telefone));
        RuleFor(x => x.Cpf).MaximumLength(14).When(x => !string.IsNullOrWhiteSpace(x.Cpf));
        RuleFor(x => x.Tipo).IsInEnum();
        RuleFor(x => x.TipoRepasse).IsInEnum();
        RuleFor(x => x.ValorRepasse).GreaterThanOrEqualTo(0);
    }
}
