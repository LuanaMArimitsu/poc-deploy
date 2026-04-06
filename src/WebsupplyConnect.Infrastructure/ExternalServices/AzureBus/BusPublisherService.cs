using Azure.Messaging.ServiceBus;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Application.Interfaces.ExternalServices;
using WebsupplyConnect.Infrastructure.Exceptions;
using System.Text.Json;
using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Application.DTOs.ExternalServices;

namespace WebsupplyConnect.Infrastructure.ExternalServices.AzureBus
{

    public class BusPublisherService : IBusPublisherService
    {
        private readonly ServiceBusClient _client;
        private readonly ILogger<BusPublisherService> _logger;
        private readonly AzureBusConfig _azureBusConfig;
        private readonly Dictionary<Type, string> _queueMappings;
        public BusPublisherService(ServiceBusClient client, ILogger<BusPublisherService> logger, IOptions<AzureBusConfig> azureBusConfig)
        {
            _client = client ?? throw new ArgumentNullException(nameof(client));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _azureBusConfig = azureBusConfig.Value ?? throw new ArgumentNullException(nameof(azureBusConfig));

            // Mapeamento seguro dos tipos para as filas
            //TODO: Adicinar as demais DTOs quando forem ser publicadas
            _queueMappings = new Dictionary<Type, string>
            {
                { typeof(WebhookMetaInboundDTO), _azureBusConfig.Queues[QueueNamesConfig.WebhookInboundMeta] },
                { typeof(MensagemOutboundDTO), _azureBusConfig.Queues[QueueNamesConfig.MensagensOutbound] },
                { typeof(MidiaOutboundDTO), _azureBusConfig.Queues[QueueNamesConfig.MidiasOutbound] }
            };
        }
        public async Task PublishAsync<T>(T message) where T : class
        {
            try
            {
                var queueName = GetQueueName(typeof(T));
                var sender = _client.CreateSender(queueName);

                var jsonMessage = JsonSerializer.Serialize(message);
                var serviceBusMessage = new ServiceBusMessage(jsonMessage);

                await sender.SendMessageAsync(serviceBusMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao publicar mensagem {Messege}", message);
                throw new InfraException("Erro ao publicar a mensagem no azure bus.", ex);
            }
        }

        private string GetQueueName(Type messageType)
        {
            if (_queueMappings.TryGetValue(messageType, out var queue))
                return queue;

            throw new InfraException($"Nenhuma fila configurada para o tipo '{messageType.Name}' e nenhuma fila padrão definida.");
        }
    }

}
