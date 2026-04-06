using WebsupplyConnect.Domain.Entities.Notificacao;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Notificacao;
using WebsupplyConnect.Infrastructure.Data.Repositories.Base;
using WebsupplyConnect.Infrastructure.Exceptions;

namespace WebsupplyConnect.Infrastructure.Data.Repositories.Notificacao
{
    internal class NotificacaoRepository(WebsupplyConnectDbContext dbContext, IUnitOfWork unitOfWork) : BaseRepository(dbContext, unitOfWork), INotificacaoRepository
    {
        public async Task<List<Domain.Entities.Notificacao.Notificacao>> GetNotificacoesByUserAsync(int usuarioId)
        {
            var resultado = await GetListByPredicateAsync<Domain.Entities.Notificacao.Notificacao>(
                w => w.UsuarioDestinatarioId == usuarioId &&
                     w.Excluido == false &&
                     w.EnviadoPush == true &&
                     !w.Conteudo.StartsWith("Status da mensagem") 
            );

            return resultado
                .OrderByDescending(e => e.DataCriacao)
                .Take(100)
                .ToList();
        }

        public async Task<List<Domain.Entities.Notificacao.Notificacao>> GetNotificacoesAtivasPorDestinatarioAsync(int usuarioId)
        {
            var resultado = await GetListByPredicateAsync<Domain.Entities.Notificacao.Notificacao>(
                w => w.UsuarioDestinatarioId == usuarioId && w.Excluido == false,
                orderBy: q => q.OrderByDescending(e => e.DataCriacao));

            return resultado;
        }

        public async Task<string> GetNotificacoesStatus(int id, bool includeDeleted = false)
        {
            var status = await GetByPredicateAsync<NotificacaoStatus>(
                             w => w.Id == id,
                             includeDeleted
            );

            return status?.Nome ?? throw new InfraException($"NotificacaoStatus com código '{id}' não encontrado.");
        }
    }
}
