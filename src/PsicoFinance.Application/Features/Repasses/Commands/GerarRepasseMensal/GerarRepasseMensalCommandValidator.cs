using FluentValidation;

namespace PsicoFinance.Application.Features.Repasses.Commands.GerarRepasseMensal;

public class GerarRepasseMensalCommandValidator : AbstractValidator<GerarRepasseMensalCommand>
{
    public GerarRepasseMensalCommandValidator()
    {
        RuleFor(x => x.MesReferencia)
            .NotEmpty().WithMessage("Mês de referência é obrigatório.")
            .Matches(@"^\d{4}-(0[1-9]|1[0-2])$").WithMessage("Mês de referência deve estar no formato YYYY-MM.");
    }
}
