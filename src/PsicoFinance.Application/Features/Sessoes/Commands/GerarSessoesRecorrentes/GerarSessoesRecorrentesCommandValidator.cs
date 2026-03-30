using FluentValidation;

namespace PsicoFinance.Application.Features.Sessoes.Commands.GerarSessoesRecorrentes;

public class GerarSessoesRecorrentesCommandValidator
    : AbstractValidator<GerarSessoesRecorrentesCommand>
{
    public GerarSessoesRecorrentesCommandValidator()
    {
        RuleFor(x => x.ContratoId)
            .NotEmpty().WithMessage("Contrato é obrigatório.");

        RuleFor(x => x.DataInicio)
            .NotEmpty().WithMessage("Data de início é obrigatória.");

        RuleFor(x => x.DataFim)
            .GreaterThan(x => x.DataInicio)
            .WithMessage("Data de término deve ser posterior à data de início.")
            .When(x => x.DataFim.HasValue);

        RuleFor(x => x.QuantidadeSessoes)
            .InclusiveBetween(1, 52).WithMessage("Quantidade deve ser entre 1 e 52 sessões.")
            .When(x => x.QuantidadeSessoes.HasValue);
    }
}
