using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Onboarding.DTOs;
using PsicoFinance.Domain.Entities;
using PsicoFinance.Domain.Enums;

namespace PsicoFinance.Application.Features.Onboarding.Commands;

public class OnboardingCommandHandler : IRequestHandler<OnboardingCommand, OnboardingResponseDto>
{
    private readonly IAppDbContext _context;
    private readonly ITenantSchemaService _schemaService;
    private readonly IPasswordHasher _passwordHasher;
    private readonly IJwtService _jwtService;
    private readonly IAuditService _auditService;

    public OnboardingCommandHandler(
        IAppDbContext context,
        ITenantSchemaService schemaService,
        IPasswordHasher passwordHasher,
        IJwtService jwtService,
        IAuditService auditService)
    {
        _context = context;
        _schemaService = schemaService;
        _passwordHasher = passwordHasher;
        _jwtService = jwtService;
        _auditService = auditService;
    }

    public async Task<OnboardingResponseDto> Handle(OnboardingCommand request, CancellationToken cancellationToken)
    {
        // 1. Verificar se CNPJ já existe (se informado)
        if (!string.IsNullOrWhiteSpace(request.Cnpj))
        {
            var cnpjExiste = await _context.Clinicas
                .AnyAsync(c => c.Cnpj == request.Cnpj, cancellationToken);

            if (cnpjExiste)
                throw new InvalidOperationException("Já existe uma clínica cadastrada com este CNPJ.");
        }

        // 2. Verificar se email admin já existe em alguma clínica
        var emailExiste = await _context.Usuarios
            .IgnoreQueryFilters()
            .AnyAsync(u => u.Email == request.EmailAdmin, cancellationToken);

        if (emailExiste)
            throw new InvalidOperationException("Já existe um usuário cadastrado com este email.");

        // 3. Criar a clínica
        var clinica = new Clinica
        {
            Id = Guid.NewGuid(),
            Nome = request.NomeClinica,
            Cnpj = request.Cnpj,
            Email = request.EmailClinica,
            Telefone = request.Telefone,
            Ativo = true
        };

        _context.Clinicas.Add(clinica);
        await _context.SaveChangesAsync(cancellationToken);

        // 4. Criar schema PostgreSQL para o tenant
        await _schemaService.CreateSchemaForTenantAsync(clinica.Id, cancellationToken);

        // 5. Criar usuário admin vinculado à clínica
        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = request.NomeAdmin,
            Email = request.EmailAdmin,
            SenhaHash = _passwordHasher.Hash(request.SenhaAdmin),
            Role = RoleUsuario.Admin,
            ClinicaId = clinica.Id,
            Ativo = true,
            UltimoAcessoEm = DateTimeOffset.UtcNow
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync(cancellationToken);

        // 6. Gerar JWT
        var accessToken = _jwtService.GenerateAccessToken(usuario);

        // 7. Registrar auditoria
        await _auditService.RegistrarAsync(new AuditLog
        {
            ClinicaId = clinica.Id,
            UsuarioId = usuario.Id,
            Acao = "Criar",
            Entidade = nameof(Clinica),
            EntidadeId = clinica.Id,
            DadosNovos = System.Text.Json.JsonSerializer.Serialize(new
            {
                clinica.Nome,
                clinica.Cnpj,
                clinica.Email,
                AdminEmail = usuario.Email
            }),
            IpOrigem = request.IpOrigem
        }, cancellationToken);

        return new OnboardingResponseDto(
            clinica.Id,
            usuario.Id,
            clinica.Nome,
            usuario.Nome,
            usuario.Email,
            accessToken);
    }
}
