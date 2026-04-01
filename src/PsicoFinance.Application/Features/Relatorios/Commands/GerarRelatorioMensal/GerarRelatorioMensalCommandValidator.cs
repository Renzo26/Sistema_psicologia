using FluentValidation;

namespace PsicoFinance.Application.Features.Relatorios.Commands.GerarRelatorioMensal;

public class GerarRelatorioMensalCommandValidator : AbstractValidator<GerarRelatorioMensalCommand>
{
    public GerarRelatorioMensalCommandValidator()
    {
        RuleFor(x => x.PsicologoId)
            .NotEmpty().WithMessage("Psicólogo é obrigatório.");

        RuleFor(x => x.Competencia)
            .NotEmpty().WithMessage("Competência é obrigatória.")
            .Matches(@"^\d{4}-(0[1-9]|1[0-2])$").WithMessage("Competência deve estar no formato YYYY-MM.");
    }
}
