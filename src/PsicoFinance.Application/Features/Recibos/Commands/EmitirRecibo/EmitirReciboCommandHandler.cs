using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Recibos.DTOs;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Recibos.Commands.EmitirRecibo;

public class EmitirReciboCommandHandler : IRequestHandler<EmitirReciboCommand, ReciboDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantProvider _tenantProvider;
    private readonly IPdfService _pdfService;
    private readonly IFileStorageService _storageService;

    public EmitirReciboCommandHandler(
        IAppDbContext context,
        ITenantProvider tenantProvider,
        IPdfService pdfService,
        IFileStorageService storageService)
    {
        _context = context;
        _tenantProvider = tenantProvider;
        _pdfService = pdfService;
        _storageService = storageService;
    }

    public async Task<ReciboDto> Handle(EmitirReciboCommand request, CancellationToken cancellationToken)
    {
        var clinicaId = _tenantProvider.ClinicaId
            ?? throw new UnauthorizedAccessException("Tenant não identificado.");

        // Busca a sessão com todos os dados necessários
        var sessao = await _context.Sessoes
            .Include(s => s.Paciente)
            .Include(s => s.Psicologo)
            .Include(s => s.Contrato)
            .Include(s => s.Clinica)
            .FirstOrDefaultAsync(s => s.Id == request.SessaoId, cancellationToken)
            ?? throw new KeyNotFoundException("Sessão não encontrada.");

        if (sessao.Status != StatusSessao.Realizada)
            throw new InvalidOperationException("Recibo só pode ser emitido para sessões realizadas.");

        // Verifica se já existe recibo para esta sessão
        var reciboExistente = await _context.Recibos
            .AnyAsync(r => r.SessaoId == request.SessaoId && r.Status != StatusRecibo.Cancelado, cancellationToken);

        if (reciboExistente)
            throw new InvalidOperationException("Já existe um recibo ativo para esta sessão.");

        // Busca lançamento financeiro vinculado à sessão
        var lancamento = await _context.LancamentosFinanceiros
            .FirstOrDefaultAsync(l => l.SessaoId == request.SessaoId && l.Status != StatusLancamento.Cancelado, cancellationToken);

        // Gera número sequencial do recibo
        var ultimoNumero = await _context.Recibos
            .Where(r => r.ClinicaId == clinicaId)
            .OrderByDescending(r => r.CriadoEm)
            .Select(r => r.NumeroRecibo)
            .FirstOrDefaultAsync(cancellationToken);

        var novoNumero = GerarProximoNumero(ultimoNumero);

        // Monta endereço da clínica
        var endereco = MontarEndereco(sessao.Clinica);

        // Gera o PDF
        var reciboData = new ReciboData(
            NumeroRecibo: novoNumero,
            ClinicaNome: sessao.Clinica.Nome,
            ClinicaCnpj: sessao.Clinica.Cnpj,
            ClinicaTelefone: sessao.Clinica.Telefone,
            ClinicaEmail: sessao.Clinica.Email,
            ClinicaEndereco: endereco,
            PacienteNome: sessao.Paciente.Nome,
            PacienteCpf: sessao.Paciente.Cpf,
            PsicologoNome: sessao.Psicologo.Nome,
            PsicologoCrp: sessao.Psicologo.Crp,
            DataSessao: sessao.Data,
            HorarioSessao: sessao.HorarioInicio,
            DuracaoMinutos: sessao.DuracaoMinutos,
            Valor: sessao.Contrato.ValorSessao,
            FormaPagamento: sessao.Contrato.FormaPagamento.ToString(),
            DataEmissao: DateOnly.FromDateTime(DateTime.UtcNow)
        );

        var pdfBytes = _pdfService.GerarRecibo(reciboData);

        // Salva o arquivo
        var fileName = $"recibo_{novoNumero}_{sessao.Data:yyyyMMdd}.pdf";
        var folder = $"{clinicaId}/recibos";
        var relativePath = await _storageService.SaveAsync(folder, fileName, pdfBytes, cancellationToken);

        // Cria o registro no banco
        var recibo = new Recibo
        {
            Id = Guid.NewGuid(),
            ClinicaId = clinicaId,
            SessaoId = request.SessaoId,
            PacienteId = sessao.PacienteId,
            LancamentoId = lancamento?.Id,
            NumeroRecibo = novoNumero,
            Valor = sessao.Contrato.ValorSessao,
            DataEmissao = DateOnly.FromDateTime(DateTime.UtcNow),
            Status = StatusRecibo.Gerado,
            ArquivoUrl = relativePath,
            ArquivoNome = fileName,
            CriadoPor = null // UsuarioId can be added to ITenantProvider later
        };

        _context.Recibos.Add(recibo);
        await _context.SaveChangesAsync(cancellationToken);

        return new ReciboDto(
            recibo.Id, recibo.NumeroRecibo, recibo.SessaoId,
            sessao.Paciente.Nome, sessao.Psicologo.Nome,
            recibo.Valor, recibo.DataEmissao, recibo.Status,
            recibo.ArquivoUrl, recibo.CriadoEm);
    }

    private static string GerarProximoNumero(string? ultimoNumero)
    {
        if (string.IsNullOrEmpty(ultimoNumero))
            return "REC-000001";

        var partes = ultimoNumero.Split('-');
        if (partes.Length == 2 && int.TryParse(partes[1], out var numero))
            return $"REC-{(numero + 1):D6}";

        return $"REC-000001";
    }

    private static string? MontarEndereco(Clinica clinica)
    {
        var partes = new List<string>();
        if (!string.IsNullOrEmpty(clinica.Logradouro))
        {
            var rua = clinica.Logradouro;
            if (!string.IsNullOrEmpty(clinica.Numero)) rua += $", {clinica.Numero}";
            if (!string.IsNullOrEmpty(clinica.Complemento)) rua += $" - {clinica.Complemento}";
            partes.Add(rua);
        }
        if (!string.IsNullOrEmpty(clinica.Bairro)) partes.Add(clinica.Bairro);
        if (!string.IsNullOrEmpty(clinica.Cidade))
        {
            var local = clinica.Cidade;
            if (!string.IsNullOrEmpty(clinica.Estado)) local += $"/{clinica.Estado}";
            partes.Add(local);
        }
        if (!string.IsNullOrEmpty(clinica.Cep)) partes.Add($"CEP: {clinica.Cep}");

        return partes.Count > 0 ? string.Join(" — ", partes) : null;
    }
}
