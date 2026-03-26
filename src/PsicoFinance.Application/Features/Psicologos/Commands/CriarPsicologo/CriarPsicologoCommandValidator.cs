using FluentValidation;

namespace PsicoFinance.Application.Features.Psicologos.Commands.CriarPsicologo;

public class CriarPsicologoCommandValidator : AbstractValidator<CriarPsicologoCommand>
{
    public CriarPsicologoCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Crp)
            .NotEmpty().WithMessage("CRP é obrigatório.")
            .MaximumLength(20).WithMessage("CRP deve ter no máximo 20 caracteres.");

        RuleFor(x => x.Email)
            .EmailAddress().WithMessage("Email inválido.")
            .MaximumLength(254)
            .When(x => !string.IsNullOrWhiteSpace(x.Email));

        RuleFor(x => x.Telefone)
            .MaximumLength(20)
            .When(x => !string.IsNullOrWhiteSpace(x.Telefone));

        RuleFor(x => x.Cpf)
            .MaximumLength(14)
            .When(x => !string.IsNullOrWhiteSpace(x.Cpf));

        RuleFor(x => x.Tipo).IsInEnum().WithMessage("Tipo inválido.");
        RuleFor(x => x.TipoRepasse).IsInEnum().WithMessage("Tipo de repasse inválido.");

        RuleFor(x => x.ValorRepasse)
            .GreaterThanOrEqualTo(0).WithMessage("Valor de repasse não pode ser negativo.");
    }
}
