using FluentValidation;
using System.Text.RegularExpressions;

namespace PsicoFinance.Application.Features.Fechamentos.Commands.RealizarFechamentoMensal;

public class RealizarFechamentoMensalCommandValidator
    : AbstractValidator<RealizarFechamentoMensalCommand>
{
    public RealizarFechamentoMensalCommandValidator()
    {
        RuleFor(x => x.MesReferencia)
            .NotEmpty().WithMessage("Mês de referência é obrigatório.")
            .Matches(@"^\d{4}-\d{2}$").WithMessage("Formato inválido. Use YYYY-MM.");

        RuleFor(x => x.Observacao)
            .MaximumLength(1000)
            .When(x => !string.IsNullOrWhiteSpace(x.Observacao));
    }
}
