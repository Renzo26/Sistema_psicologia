using FluentValidation.TestHelper;
using PsicoFinance.Application.Features.Pacientes.Commands.CriarPaciente;

namespace PsicoFinance.Tests.Pacientes;

public class CriarPacienteCommandValidatorTests
{
    private readonly CriarPacienteCommandValidator _validator = new();

    private static CriarPacienteCommand Valido() => new(
        Nome: "Maria Silva", Cpf: "123.456.789-00", Email: "maria@email.com",
        Telefone: "(11) 99999-0000", DataNascimento: new DateOnly(1990, 5, 15),
        ResponsavelNome: null, ResponsavelTelefone: null, Observacoes: null);

    [Fact] public void Valido_SemErros() => _validator.TestValidate(Valido()).ShouldNotHaveAnyValidationErrors();

    [Fact] public void NomeVazio_Erro() => _validator.TestValidate(Valido() with { Nome = "" }).ShouldHaveValidationErrorFor(x => x.Nome);

    [Fact] public void NomeLongo_Erro() => _validator.TestValidate(Valido() with { Nome = new string('A', 151) }).ShouldHaveValidationErrorFor(x => x.Nome);

    [Fact] public void EmailInvalido_Erro() => _validator.TestValidate(Valido() with { Email = "invalido" }).ShouldHaveValidationErrorFor(x => x.Email);

    [Fact] public void EmailNulo_SemErro() => _validator.TestValidate(Valido() with { Email = null }).ShouldNotHaveValidationErrorFor(x => x.Email);

    [Fact] public void DataFutura_Erro() => _validator.TestValidate(Valido() with { DataNascimento = DateOnly.FromDateTime(DateTime.Today.AddDays(1)) }).ShouldHaveValidationErrorFor(x => x.DataNascimento);

    [Fact] public void ObservacoesLongas_Erro() => _validator.TestValidate(Valido() with { Observacoes = new string('A', 2001) }).ShouldHaveValidationErrorFor(x => x.Observacoes);

    [Fact] public void CpfNulo_SemErro() => _validator.TestValidate(Valido() with { Cpf = null }).ShouldNotHaveValidationErrorFor(x => x.Cpf);
}
