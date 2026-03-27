using FluentValidation.TestHelper;
using PsicoFinance.Application.Features.Contratos.Commands.CriarContrato;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Tests.Contratos;

public class CriarContratoCommandValidatorTests
{
    private readonly CriarContratoCommandValidator _validator = new();

    private static CriarContratoCommand Valido() => new(
        PacienteId: Guid.NewGuid(), PsicologoId: Guid.NewGuid(),
        ValorSessao: 150m, FormaPagamento: FormaPagamento.Pix,
        Frequencia: FrequenciaContrato.Semanal, DiaSemanaSessao: DiaSemana.Segunda,
        HorarioSessao: new TimeOnly(14, 0), DuracaoMinutos: 50,
        CobraFaltaInjustificada: true, CobraFaltaJustificada: false,
        DataInicio: DateOnly.FromDateTime(DateTime.Today),
        DataFim: null, PlanoContaId: null, Observacoes: null);

    [Fact]
    public void Valido_SemErros() =>
        _validator.TestValidate(Valido()).ShouldNotHaveAnyValidationErrors();

    [Fact]
    public void PacienteVazio_Erro() =>
        _validator.TestValidate(Valido() with { PacienteId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.PacienteId);

    [Fact]
    public void PsicologoVazio_Erro() =>
        _validator.TestValidate(Valido() with { PsicologoId = Guid.Empty })
            .ShouldHaveValidationErrorFor(x => x.PsicologoId);

    [Fact]
    public void ValorZero_Erro() =>
        _validator.TestValidate(Valido() with { ValorSessao = 0 })
            .ShouldHaveValidationErrorFor(x => x.ValorSessao);

    [Fact]
    public void ValorNegativo_Erro() =>
        _validator.TestValidate(Valido() with { ValorSessao = -10 })
            .ShouldHaveValidationErrorFor(x => x.ValorSessao);

    [Fact]
    public void DuracaoMenor15_Erro() =>
        _validator.TestValidate(Valido() with { DuracaoMinutos = 10 })
            .ShouldHaveValidationErrorFor(x => x.DuracaoMinutos);

    [Fact]
    public void DuracaoMaior240_Erro() =>
        _validator.TestValidate(Valido() with { DuracaoMinutos = 300 })
            .ShouldHaveValidationErrorFor(x => x.DuracaoMinutos);

    [Fact]
    public void DataFimAnteriorInicio_Erro() =>
        _validator.TestValidate(Valido() with
        {
            DataInicio = new DateOnly(2025, 6, 1),
            DataFim = new DateOnly(2025, 5, 1)
        }).ShouldHaveValidationErrorFor(x => x.DataFim);

    [Fact]
    public void DataFimNula_SemErro() =>
        _validator.TestValidate(Valido() with { DataFim = null })
            .ShouldNotHaveValidationErrorFor(x => x.DataFim);

    [Fact]
    public void ObservacoesLongas_Erro() =>
        _validator.TestValidate(Valido() with { Observacoes = new string('A', 2001) })
            .ShouldHaveValidationErrorFor(x => x.Observacoes);

    [Fact]
    public void ObservacoesNulas_SemErro() =>
        _validator.TestValidate(Valido() with { Observacoes = null })
            .ShouldNotHaveValidationErrorFor(x => x.Observacoes);
}
