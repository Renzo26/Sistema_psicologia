using FluentAssertions;
using FluentValidation.TestHelper;
using PsicoFinance.Application.Features.Clinicas.Commands.CriarClinica;

namespace PsicoFinance.Tests.Clinicas;

public class CriarClinicaCommandValidatorTests
{
    private readonly CriarClinicaCommandValidator _validator = new();

    private CriarClinicaCommand CriarCommand(
        string nome = "Clínica Teste",
        string email = "teste@clinica.com",
        string? cnpj = null,
        string? estado = "SP") => new(
        Nome: nome,
        Cnpj: cnpj,
        Email: email,
        Telefone: "(11) 99999-0000",
        Cep: "01310-100",
        Logradouro: "Av. Paulista",
        Numero: "1000",
        Complemento: null,
        Bairro: "Bela Vista",
        Cidade: "São Paulo",
        Estado: estado);

    [Fact]
    public void Validar_CommandValido_PassaSemErros()
    {
        var result = _validator.TestValidate(CriarCommand());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void Validar_NomeVazio_Falha()
    {
        var result = _validator.TestValidate(CriarCommand(nome: ""));
        result.ShouldHaveValidationErrorFor(x => x.Nome);
    }

    [Fact]
    public void Validar_NomeExcedeLimite_Falha()
    {
        var result = _validator.TestValidate(CriarCommand(nome: new string('A', 151)));
        result.ShouldHaveValidationErrorFor(x => x.Nome);
    }

    [Fact]
    public void Validar_EmailVazio_Falha()
    {
        var result = _validator.TestValidate(CriarCommand(email: ""));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validar_EmailInvalido_Falha()
    {
        var result = _validator.TestValidate(CriarCommand(email: "nao-e-email"));
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void Validar_CnpjExcedeLimite_Falha()
    {
        var result = _validator.TestValidate(CriarCommand(cnpj: new string('1', 19)));
        result.ShouldHaveValidationErrorFor(x => x.Cnpj);
    }

    [Fact]
    public void Validar_CnpjNulo_Permitido()
    {
        var result = _validator.TestValidate(CriarCommand(cnpj: null));
        result.ShouldNotHaveValidationErrorFor(x => x.Cnpj);
    }

    [Fact]
    public void Validar_EstadoExcedeLimite_Falha()
    {
        var result = _validator.TestValidate(CriarCommand(estado: "SPP"));
        result.ShouldHaveValidationErrorFor(x => x.Estado);
    }
}
