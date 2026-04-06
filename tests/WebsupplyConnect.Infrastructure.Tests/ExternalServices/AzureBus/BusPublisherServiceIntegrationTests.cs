using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Infrastructure.ExternalServices.AzureBus;
using System.Text;
using System.Text.Json;
using WebsupplyConnect.Application.DTOs.ExternalServices;

namespace WebsupplyConnect.Infrastructure.Tests.ExternalServices.AzureBus
{
    // Classe de teste de integração que implementa IAsyncLifetime para gerenciar recursos antes e depois dos testes
    public class BusPublisherServiceIntegrationTests : IAsyncLifetime
    {
        // Declaração dos objetos necessários
        private readonly ServiceBusClient _client;
        private readonly BusPublisherService _service;
        private readonly ServiceBusReceiver _receiver;
        private readonly string _queueNameWebhook;
        private readonly string _connectionString;

        // Construtor do teste de integração
        public BusPublisherServiceIntegrationTests()
        {
            // Carrega as configurações do arquivo appsettings.Test.json
            var config = new ConfigurationBuilder()
                .AddJsonFile("appsettings.Test.json")
                .Build();

            // Lê a connection string do Azure Service Bus do arquivo de configuração
            _connectionString = config["AzureBusConnection:EndpointAzureBus"]!;

            // Lê o nome da fila webhook do arquivo de configuração
            _queueNameWebhook = config["AzureBusConnection:Queues:WebhookMeta"]!;

            // Cria um objeto de configuração do AzureBusConfig com os mapeamentos de filas
            var azureBusConfig = new AzureBusConfig
            {
                Queues = new Dictionary<string, string>
                {
                    { QueueNamesConfig.WebhookInboundMeta, _queueNameWebhook },
                    { QueueNamesConfig.MidiasInbound, _queueNameWebhook }
                }
            };

            // Instancia um client real do Service Bus com a connection string
            _client = new ServiceBusClient(_connectionString);

            // Cria um logger para o serviço, que escreve no console
            var logger = LoggerFactory.Create(builder => builder.AddConsole())
                                      .CreateLogger<BusPublisherService>();

            // Instancia o serviço que será testado
            _service = new BusPublisherService(_client, logger, Options.Create(azureBusConfig));

            // Cria um receiver que escuta a fila webhook
            _receiver = _client.CreateReceiver(_queueNameWebhook);
        }

        // Teste de integração que valida se a mensagem foi publicada corretamente no Service Bus
        [Fact]
        public async Task PublishAsync_DevePublicarMensagemNoServiceBus()
        {
            // Arrange: cria uma mensagem para ser publicada
            var message = new WebhookMetaInboundDTO("1", "payload-teste", "signature-teste");

            // Act: executa a publicação da mensagem no Service Bus
            await _service.PublishAsync(message);

            // Assert: tenta receber a mensagem da fila webhook
            var receivedMessage = await _receiver.ReceiveMessageAsync(TimeSpan.FromSeconds(10));

            // Verifica se a mensagem foi recebida
            receivedMessage.Should().NotBeNull("Mensagem deveria ter sido recebida da fila");

            // Desserializa o conteúdo da mensagem recebida
            var body = Encoding.UTF8.GetString(receivedMessage.Body);
            var deserialized = JsonSerializer.Deserialize<WebhookMetaInboundDTO>(body);

            // Valida se os dados recebidos são iguais aos enviados
            deserialized.Should().NotBeNull();
            deserialized!.IdExterno.Should().Be(message.IdExterno);
            deserialized.Payload.Should().Be(message.Payload);
            deserialized.AssinaturaHMAC.Should().Be(message.AssinaturaHMAC);

            // Faz o complete da mensagem, removendo-a da fila
            await _receiver.CompleteMessageAsync(receivedMessage);
        }

        // Método chamado automaticamente após a execução dos testes para liberar recursos
        public async Task DisposeAsync()
        {
            await _receiver.CloseAsync();
            await _client.DisposeAsync();
        }

        // Método chamado antes da execução dos testes (não precisa fazer nada nesse cenário)
        public Task InitializeAsync() => Task.CompletedTask;
    }
}