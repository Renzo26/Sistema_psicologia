using FluentValidation;

namespace PsicoFinance.Application.Features.Recibos.Commands.EmitirRecibo;

public class EmitirReciboCommandValidator : AbstractValidator<EmitirReciboCommand>
{
    public EmitirReciboCommandValidator()
    {
        RuleFor(x => x.SessaoId)
            .NotEmpty().WithMessage("ID da sessão é obrigatório.");
    }
}
