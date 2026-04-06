using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Usuario;

namespace WebsupplyConnect.Infrastructure.ExternalServices.WhatsApp
{
    public class WhatsAppInatividadeService(ILogger<WhatsAppInatividadeService> logger, IConversaReaderService conversaReaderService, IConversaWriterService conversaWriterService, IUsuarioReaderService usuarioReaderService, IMensagemWriterService mensagemWriterService) : IWhatsAppInatividadeService
    {
        private readonly ILogger<WhatsAppInatividadeService> _logger = logger;
        private readonly IConversaReaderService _conversaReaderService = conversaReaderService;
        private readonly IConversaWriterService _conversaWriterService = conversaWriterService;
        private readonly IUsuarioReaderService _usuarioReaderService = usuarioReaderService;
        private readonly IMensagemWriterService _mensagemWriterService = mensagemWriterService;

        private const int TAMANHO_LOTE = 50; // Processa 50 conversas por vez
        private const int MAX_PARALELO = 5; // Máximo 5 mensagens simultâneas
        private const int DELAY_ENTRE_LOTES_MS = 200; // Pausa de 200ms entre lotes

        public async Task ProcessarInatividade()
        {
            var bot = await _usuarioReaderService.GetUsuarioBot();
            if (bot == null)
            {
                _logger.LogWarning("Bot não encontrado ao processar inatividade");
                return;
            }

            await ProcessarEncerramentos(bot.Id);
            await ProcessarAvisosInatividade(bot.Id);
        }

        private async Task ProcessarAvisosInatividade(int botId)
        {
            var totalProcessados = 0;
            var totalErros = 0;
            var pagina = 0;

            while (true)
            {
                // Busca um lote de conversas do banco
                var loteConversas = await _conversaReaderService
                    .GetConversasComInatividade(botId, pagina, TAMANHO_LOTE);

                // Se não tem mais conversas, para o loop
                if (loteConversas == null || loteConversas.Count == 0)
                {
                    break;
                }

                // Processa as conversas do lote em paralelo
                foreach(var conversa in loteConversas)
                {
                    try
                    {
                        var dto = new MensagemRequestDTO
                        {
                            Midia = false,
                            Template = false,
                            Conteudo = "Oi! 😊\r\nPara continuar, preciso da sua resposta.\r\nCaso não haja retorno em até 5 minutos, o atendimento será encerrado. Tudo bem?",
                            TipoMensagem = "TEXT",
                            LeadId = conversa.LeadId,
                            UsuarioId = conversa.UsuarioId,
                            EhAviso = true
                        };

                        await _mensagemWriterService.ProcessarMensagemAsync(dto);

                        // Incrementa contador de sucesso (thread-safe)
                        Interlocked.Increment(ref totalProcessados);
                    }
                    catch (Exception ex)
                    {
                        // Incrementa contador de erros (thread-safe)
                        Interlocked.Increment(ref totalErros);

                        _logger.LogError(
                            ex,
                            "Erro ao enviar aviso de inatividade para Lead {LeadId}",
                            conversa.LeadId
                        );
                    }
                };

                // Próxima página
                pagina++;

                // Pausa pequena entre lotes para não sobrecarregar
                // Remove se não precisar
                if (loteConversas.Count == TAMANHO_LOTE) // Só pausa se tem mais lotes
                {
                    await Task.Delay(DELAY_ENTRE_LOTES_MS);
                }
            }
        }

        private async Task ProcessarEncerramentos(int botId)
        {
            var totalProcessados = 0;
            var totalErros = 0;
            var pagina = 0;

            while (true)
            {
                // Busca um lote de conversas do banco
                var loteConversas = await _conversaReaderService
                    .GetConversasComAviso(botId, pagina, TAMANHO_LOTE);

                // Se não tem mais conversas, para o loop
                if (loteConversas == null || loteConversas.Count == 0)
                {
                    break;
                }


                foreach(var conversa in loteConversas)
                {
                    try
                    {
                        var dto = new MensagemRequestDTO
                        {
                            Midia = false,
                            Template = false,
                            Conteudo = "Oi! 😊\r\nVi que você não conseguiu responder a tempo.\r\nVou encerrar este atendimento por agora, mas quando precisar, é só chamar — estarei por aqui!",
                            TipoMensagem = "TEXT",
                            LeadId = conversa.LeadId,
                            UsuarioId = conversa.UsuarioId,
                            EhAviso = true
                        };

                        await _mensagemWriterService.ProcessarMensagemAsync(dto);
                        await _conversaWriterService.EncerrarConversaAsync(conversa.Id, conversa.UsuarioId);

                        Interlocked.Increment(ref totalProcessados);
                    }
                    catch (Exception ex)
                    {
                        Interlocked.Increment(ref totalErros);

                        _logger.LogError(
                            ex,
                            "Erro ao encerrar conversa {ConversaId} para Lead {LeadId}",
                            conversa.Id,
                            conversa.LeadId
                        );
                    }
                };

                pagina++;

                // Pausa entre lotes
                if (loteConversas.Count == TAMANHO_LOTE)
                {
                    await Task.Delay(DELAY_ENTRE_LOTES_MS);
                }
            }
        }

    }
}
