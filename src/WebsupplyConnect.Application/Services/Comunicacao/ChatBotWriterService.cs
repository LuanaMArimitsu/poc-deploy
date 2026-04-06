using Microsoft.Extensions.Logging;
using System.Text.Json;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Application.Services.Comunicacao
{
    public class ChatBotWriterService(ILogger<ChatBotWriterService> logger, IRedisCacheService redisCacheService) : IChatBotWriterService
    {
        private readonly string _redisKeyPrefix = "Conversation:";
        private readonly ILogger<ChatBotWriterService> _logger = logger;
        private readonly IRedisCacheService _redisCacheService = redisCacheService;

        public async Task<bool> CreateHistoryToBot(CreateHistoryBotObjectDTO botObject)
        {
            try
            {
                var redisKey = $"{_redisKeyPrefix}{botObject.ConversaId}";

                List<MessageRedisDTO> messageRedisList = [];

                if (botObject.MensagensAntigas == null || botObject.MensagensAntigas.Count == 0)
                {
                    var messageRedis = new MessageRedisDTO
                    {
                        Message = botObject.Mensagem,
                        Sender = "Customer",
                        SenderIsBot = false,
                        SentOn = TimeHelper.GetBrasiliaTime()
                    };

                    messageRedisList.Add(messageRedis);
                }
                else
                {
                    // Caso existam mensagens antigas, adiciona todas elas ao histórico
                    foreach (var item in botObject.MensagensAntigas)
                    {
                        var messageRedis = new MessageRedisDTO
                        {
                            Message = item.Conteudo!,
                            Sender = "Customer",
                            SenderIsBot = false,
                            SentOn = item.DataEnvio
                        };

                        messageRedisList.Add(messageRedis);
                    }

                    //  Caso nenhuma das antigas tenha o mesmo conteúdo da atual, adiciona também a mensagem atual
                    if (!botObject.MensagensAntigas.Any(x => x.Conteudo == botObject.Mensagem))
                    {
                        messageRedisList.Add(new MessageRedisDTO
                        {
                            Message = botObject.Mensagem,
                            Sender = "Customer",
                            SenderIsBot = false,
                            SentOn = TimeHelper.GetBrasiliaTime()
                        });
                    }
                }

                var leadInformation = new LeadInformationDTO
                {
                    CustomerId = botObject.LeadId,
                    CompanyId = botObject.EmpresaId
                };

                var createHistoryToBot = new CreateHistoryToBotDTO
                {
                    ChatBotId = botObject.ChatBotId,
                    LeadInformation = leadInformation,
                    MessagesHistory = messageRedisList,
                    Branches = botObject.Filiais,
                    CompanyName = botObject.GrupoEmpresa
                };

                var cacheJson = JsonSerializer.Serialize(createHistoryToBot, new JsonSerializerOptions
                {
                    WriteIndented = true
                });

                _logger.LogInformation(
                    "Objeto COMPLETO que será salvo no Redis ({RedisKey}):\n{CacheJson}",
                    redisKey,
                    cacheJson
                );

                await _redisCacheService.SetAsync(redisKey, createHistoryToBot, TimeSpan.FromMinutes(50));
                return true;

            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao criar histórico para o bot.");
                return false;
            }
        }

        public async Task<CreateHistoryToBotDTO?> GetHistoryToBot(int conversaId)
        {
            try
            {
                var redisKey = $"{_redisKeyPrefix}{conversaId}";
                var objCache = await _redisCacheService.GetAsync<CreateHistoryToBotDTO>(redisKey);
                if (objCache == null) { return null; }
                return objCache;
            }
            catch (AppException ex)
            {
                _logger.LogError(ex, "Erro ao buscar histórico de informações do bot.");
                throw;
            }
        }

        public async Task UpdateHistoryBot(MessageRedisDTO novaMensagem, int conversaId)
        {
            try
            {
                var redisKey = $"{_redisKeyPrefix}{conversaId}";
                var objCache = await _redisCacheService.GetAsync<CreateHistoryToBotDTO>(redisKey);

                if (objCache == null)
                {
                    _logger.LogWarning("Histórico não encontrado para conversa {ConversaId}", conversaId);
                    return;
                }

                objCache.MessagesHistory.Add(novaMensagem);

                Console.WriteLine(JsonSerializer.Serialize(objCache));

                await _redisCacheService.SetAsync(redisKey, objCache, TimeSpan.FromMinutes(30));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao atualizar histórico de informações do bot.");
                throw;
            }
        }

    }
}
