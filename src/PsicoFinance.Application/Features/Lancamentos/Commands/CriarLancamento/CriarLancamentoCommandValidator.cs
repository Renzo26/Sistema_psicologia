using FluentValidation;

namespace PsicoFinance.Application.Features.Lancamentos.Commands.CriarLancamento;

public class CriarLancamentoCommandValidator : AbstractValidator<CriarLancamentoCommand>
{
    public CriarLancamentoCommandValidator()
    {
        RuleFor(x => x.Descricao)
            .NotEmpty().WithMessage("Descrição é obrigatória.")
            .MaximumLength(200).WithMessage("Descrição deve ter no máximo 200 caracteres.");

        RuleFor(x => x.Valor)
            .GreaterThan(0).WithMessage("Valor deve ser maior que zero.");

        RuleFor(x => x.Tipo)
            .IsInEnum().WithMessage("Tipo inválido.");

        RuleFor(x => x.DataVencimento)
            .NotEmpty().WithMessage("Data de vencimento é obrigatória.");

        RuleFor(x => x.Competencia)
            .NotEmpty().WithMessage("Competência é obrigatória.")
            .Matches(@"^\d{4}-(0[1-9]|1[0-2])$").WithMessage("Competência deve estar no formato YYYY-MM.");

        RuleFor(x => x.PlanoContaId)
            .NotEmpty().WithMessage("Plano de conta é obrigatório.");

        RuleFor(x => x.Observacao)
            .MaximumLength(500).WithMessage("Observação deve ter no máximo 500 caracteres.")
            .When(x => x.Observacao is not null);
    }
}
