using FluentValidation;

namespace PsicoFinance.Application.Features.RelatoriosBI.Commands.AtualizarRelatorioPersonalizado;

public class AtualizarRelatorioPersonalizadoValidator : AbstractValidator<AtualizarRelatorioPersonalizadoCommand>
{
    public AtualizarRelatorioPersonalizadoValidator()
    {
        RuleFor(x => x.Id)
            .NotEmpty().WithMessage("Id é obrigatório.");

        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome é obrigatório.")
            .MinimumLength(2).WithMessage("Nome deve ter ao menos 2 caracteres.")
            .MaximumLength(100).WithMessage("Nome deve ter no máximo 100 caracteres.");

        RuleFor(x => x.Agrupamento)
            .Must(a => a == null || new[] { "dia", "semana", "mes", "trimestre", "ano" }.Contains(a))
            .WithMessage("Agrupamento deve ser: dia, semana, mes, trimestre ou ano.");

        RuleFor(x => x.Ordenacao)
            .Must(o => o == null || o == "asc" || o == "desc")
            .WithMessage("Ordenação deve ser: asc ou desc.");

        RuleFor(x => x.Filtros)
            .NotNull().WithMessage("Filtros são obrigatórios.");
    }
}
