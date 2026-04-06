using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Notificacao;
using WebsupplyConnect.Application.Interfaces.Notificacao;
using WebsupplyConnect.Domain.Interfaces.Notificacao;

namespace WebsupplyConnect.Application.Services.Notificacao
{
    public class NotificacaoReaderService(ILogger<NotificacaoReaderService> logger, INotificacaoRepository notificacaoRepository) : INotificacaoReaderService

    {
        private readonly ILogger<NotificacaoReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly INotificacaoRepository _notificacaoRepository = notificacaoRepository ?? throw new ArgumentNullException(nameof(notificacaoRepository));
        public async Task<List<NotificacaoListaDTO>> NotificacoesSyncAsync(int usuarioId)
        {
            try
            {
                if (usuarioId <= 0)
                    throw new AppException("ID do usuário deve ser maior que zero.");

                // Busca todas as conversas do usuário
                List<WebsupplyConnect.Domain.Entities.Notificacao.Notificacao> notificacoes = await _notificacaoRepository.GetNotificacoesByUserAsync(usuarioId);

                if (notificacoes == null)
                    return [];

                var resultado = new List<NotificacaoListaDTO>();

                foreach (var notificacao in notificacoes)
                {
                    try
                    {
                        if (notificacao == null)
                            continue;

                        var notificacaoStatus = await _notificacaoRepository.GetNotificacoesStatus(notificacao.StatusId, false);

                        var notificacaoSincronizada = new NotificacaoListaDTO
                        {
                            Id = notificacao.Id,
                            Type = notificacao.TipoEntidadeAlvo,
                            Title = notificacao.Titulo,
                            Content = notificacao.Conteudo,
                            Timestamp = notificacao.DataHora,
                            Status = notificacaoStatus
                        };

                        resultado.Add(notificacaoSincronizada);
                    }
                    catch (Exception ex)
                    {
                        _logger.LogError(ex, "Erro ao processar notificação {NotificacaoId} para usuário {UsuarioId}",
                            notificacao?.Id, usuarioId);
                    }
                }

                return resultado;
            }
            catch (Exception ex)
            {
                throw new AppException("Erro interno ao sincronizar conversas", ex);
            }
        }
    }
}
