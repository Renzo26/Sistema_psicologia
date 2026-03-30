using FluentValidation;

namespace PsicoFinance.Application.Features.PlanosConta.Commands.AtualizarPlanoConta;

public class AtualizarPlanoContaCommandValidator : AbstractValidator<AtualizarPlanoContaCommand>
{
    public AtualizarPlanoContaCommandValidator()
    {
        RuleFor(x => x.Id).NotEmpty().WithMessage("Id é obrigatório.");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("Tipo inválido. Use Receita ou Despesa.");

        RuleFor(x => x.Descricao)
            .MaximumLength(300).WithMessage("Descrição deve ter no máximo 300 caracteres.")
            .When(x => x.Descricao is not null);
    }
}
