using MediatR;

namespace PsicoFinance.Application.Features.RelatoriosBI.Commands.MarcarFavorito;

public record MarcarFavoritoCommand(Guid Id, bool Favorito) : IRequest<Unit>;
