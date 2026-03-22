using MediatR;
using Microsoft.EntityFrameworkCore;
using PsicoFinance.Application.Common.Interfaces;
using PsicoFinance.Application.Features.Auth.DTOs;
using PsicoFinance.Domain.Entities;

namespace PsicoFinance.Application.Features.Auth.Commands.Register;

public class RegisterCommandHandler : IRequestHandler<RegisterCommand, UsuarioDto>
{
    private readonly IAppDbContext _context;
    private readonly IPasswordHasher _passwordHasher;

    public RegisterCommandHandler(IAppDbContext context, IPasswordHasher passwordHasher)
    {
        _context = context;
        _passwordHasher = passwordHasher;
    }

    public async Task<UsuarioDto> Handle(RegisterCommand request, CancellationToken cancellationToken)
    {
        var emailExiste = await _context.Usuarios
            .AnyAsync(u => u.Email == request.Email && u.ClinicaId == request.ClinicaId, cancellationToken);

        if (emailExiste)
            throw new ArgumentException("Já existe um usuário com este email nesta clínica.");

        var usuario = new Usuario
        {
            Id = Guid.NewGuid(),
            Nome = request.Nome,
            Email = request.Email,
            SenhaHash = _passwordHasher.Hash(request.Senha),
            Role = request.Role,
            ClinicaId = request.ClinicaId,
            PsicologoId = request.PsicologoId,
            Ativo = true
        };

        _context.Usuarios.Add(usuario);
        await _context.SaveChangesAsync(cancellationToken);

        return new UsuarioDto(
            Id: usuario.Id,
            Nome: usuario.Nome,
            Email: usuario.Email,
            Role: usuario.Role,
            ClinicaId: usuario.ClinicaId,
            Ativo: usuario.Ativo,
            UltimoAcessoEm: usuario.UltimoAcessoEm
        );
    }
}
