using FluentValidation;

namespace PsicoFinance.Application.Features.Clinicas.Commands.CriarClinica;

public class CriarClinicaCommandValidator : AbstractValidator<CriarClinicaCommand>
{
    public CriarClinicaCommandValidator()
    {
        RuleFor(x => x.Nome)
            .NotEmpty().WithMessage("Nome da clínica é obrigatório.")
            .MaximumLength(150).WithMessage("Nome deve ter no máximo 150 caracteres.");

        RuleFor(x => x.Email)
            .NotEmpty().WithMessage("Email é obrigatório.")
            .EmailAddress().WithMessage("Email inválido.")
            .MaximumLength(254).WithMessage("Email deve ter no máximo 254 caracteres.");

        RuleFor(x => x.Cnpj)
            .MaximumLength(18).WithMessage("CNPJ deve ter no máximo 18 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Cnpj));

        RuleFor(x => x.Telefone)
            .MaximumLength(20).WithMessage("Telefone deve ter no máximo 20 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Telefone));

        RuleFor(x => x.Cep)
            .MaximumLength(10).WithMessage("CEP deve ter no máximo 10 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Cep));

        RuleFor(x => x.Logradouro)
            .MaximumLength(200).WithMessage("Logradouro deve ter no máximo 200 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Logradouro));

        RuleFor(x => x.Cidade)
            .MaximumLength(100).WithMessage("Cidade deve ter no máximo 100 caracteres.")
            .When(x => !string.IsNullOrWhiteSpace(x.Cidade));

        RuleFor(x => x.Estado)
            .MaximumLength(2).WithMessage("Estado deve ter 2 caracteres (UF).")
            .When(x => !string.IsNullOrWhiteSpace(x.Estado));
    }
}
