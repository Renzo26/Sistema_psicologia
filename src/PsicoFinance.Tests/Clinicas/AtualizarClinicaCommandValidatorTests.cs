using FluentAssertions;
using FluentValidation.TestHelper;
using PsicoFinance.Application.Features.Clinicas.Commands.AtualizarClinica;

namespace PsicoFinance.Tests.Clinicas;

public class AtualizarClinicaCommandValidatorTests
{
    private readonly AtualizarClinicaCommandValidator _validator = new();

    private static AtualizarClinicaCommand CriarCommandValido() => new(
        Nome: "Clínica Teste",
        Cnpj: "12.345.678/0001-90",
        Email: "contato@clinica.com",
        Telefone: "(11) 99999-0000",
        Cep: "01310-100",
        Logradouro: "Av. Paulista",
        Numero: "1000",
        Complemento: "Sala 101",
        Bairro: "Bela Vista",
        Cidade: "São Paulo",
        Estado: "SP",
        HorarioEnvioAlerta: new TimeOnly(8, 0),
        WebhookN8nUrl: null,
        Timezone: "America/Sao_Paulo");

    [Fact]
    public void Valido_NaoDeveRetornarErros()
    {
        var result = _validator.TestValidate(CriarCommandValido());
        result.ShouldNotHaveAnyValidationErrors();
    }

    [Fact]
    public void NomeVazio_DeveRetornarErro()
    {
        var cmd = CriarCommandValido() with { Nome = "" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Nome);
    }

    [Fact]
    public void NomeMuitoLongo_DeveRetornarErro()
    {
        var cmd = CriarCommandValido() with { Nome = new string('A', 151) };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Nome);
    }

    [Fact]
    public void EmailVazio_DeveRetornarErro()
    {
        var cmd = CriarCommandValido() with { Email = "" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void EmailInvalido_DeveRetornarErro()
    {
        var cmd = CriarCommandValido() with { Email = "nao-e-email" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Email);
    }

    [Fact]
    public void CnpjMuitoLongo_DeveRetornarErro()
    {
        var cmd = CriarCommandValido() with { Cnpj = new string('1', 19) };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Cnpj);
    }

    [Fact]
    public void EstadoMaisDe2Chars_DeveRetornarErro()
    {
        var cmd = CriarCommandValido() with { Estado = "SAO" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Estado);
    }

    [Fact]
    public void TimezoneVazio_DeveRetornarErro()
    {
        var cmd = CriarCommandValido() with { Timezone = "" };
        var result = _validator.TestValidate(cmd);
        result.ShouldHaveValidationErrorFor(x => x.Timezone);
    }

    [Fact]
    public void CnpjNulo_NaoDeveRetornarErro()
    {
        var cmd = CriarCommandValido() with { Cnpj = null };
        var result = _validator.TestValidate(cmd);
        result.ShouldNotHaveValidationErrorFor(x => x.Cnpj);
    }
}
