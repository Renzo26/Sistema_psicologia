using FluentValidation.TestHelper;
using PsicoFinance.Application.Features.Psicologos.Commands.CriarPsicologo;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Tests.Psicologos;

public class CriarPsicologoCommandValidatorTests
{
    private readonly CriarPsicologoCommandValidator _validator = new();

    private static CriarPsicologoCommand Valido() => new(
        Nome: "Dr. João", Crp: "06/12345", Email: "joao@psi.com",
        Telefone: "(11) 99999-0000", Cpf: "123.456.789-00",
        Tipo: TipoPsicologo.Pj, TipoRepasse: TipoRepasse.Percentual, ValorRepasse: 40,
        Banco: "Itaú", Agencia: "0001", Conta: "12345-6", PixChave: "joao@pix.com");

    [Fact] public void Valido_SemErros() => _validator.TestValidate(Valido()).ShouldNotHaveAnyValidationErrors();

    [Fact] public void NomeVazio_Erro() => _validator.TestValidate(Valido() with { Nome = "" }).ShouldHaveValidationErrorFor(x => x.Nome);

    [Fact] public void CrpVazio_Erro() => _validator.TestValidate(Valido() with { Crp = "" }).ShouldHaveValidationErrorFor(x => x.Crp);

    [Fact] public void EmailInvalido_Erro() => _validator.TestValidate(Valido() with { Email = "invalido" }).ShouldHaveValidationErrorFor(x => x.Email);

    [Fact] public void EmailNulo_SemErro() => _validator.TestValidate(Valido() with { Email = null }).ShouldNotHaveValidationErrorFor(x => x.Email);

    [Fact] public void ValorRepasseNegativo_Erro() => _validator.TestValidate(Valido() with { ValorRepasse = -1 }).ShouldHaveValidationErrorFor(x => x.ValorRepasse);

    [Fact] public void NomeLongo_Erro() => _validator.TestValidate(Valido() with { Nome = new string('A', 151) }).ShouldHaveValidationErrorFor(x => x.Nome);
}
