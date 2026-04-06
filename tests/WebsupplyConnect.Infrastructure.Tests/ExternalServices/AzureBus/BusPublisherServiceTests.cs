// Importações necessárias para testes, mocking e serialização
using Azure.Messaging.ServiceBus;
using FluentAssertions;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Moq;
using WebsupplyConnect.Application.Configuration;
using WebsupplyConnect.Infrastructure.Exceptions;
using WebsupplyConnect.Infrastructure.ExternalServices.AzureBus;
using System.Text;
using System.Text.Json;
using Xunit;
using WebsupplyConnect.Application.DTOs.ExternalServices;

namespace WebsupplyConnect.Infrastructure.Tests.ExternalServices.AzureBus
{
    public class BusPublisherServiceTests
    {
        //  Mocks para simular as dependências
        private readonly Mock<ServiceBusClient> _serviceBusClientMock;
        private readonly Mock<ServiceBusSender> _serviceBusSenderMock;
        private readonly Mock<ILogger<BusPublisherService>> _loggerMock;

        // Serviço que está sendo testado
        private readonly BusPublisherService _service;

        // Setup inicial dos testes
        public BusPublisherServiceTests()
        {
            // Instanciando os mocks
            _serviceBusClientMock = new Mock<ServiceBusClient>();
            _serviceBusSenderMock = new Mock<ServiceBusSender>();
            _loggerMock = new Mock<ILogger<BusPublisherService>>();

            // Configuração fake das filas
            var config = Options.Create(new AzureBusConfig
            {
                Queues = new Dictionary<string, string>
                {
                    { QueueNamesConfig.WebhookInboundMeta, "fila-webhook" }, // fila específica para WebhookMetaDTO
                    { QueueNamesConfig.MidiasInbound, "fila-midias-meta" }
                }
            });

            // Sempre que CreateSender for chamado, retorna o sender mockado
            _serviceBusClientMock
                .Setup(x => x.CreateSender(It.IsAny<string>()))
                .Returns(_serviceBusSenderMock.Object);

            // Instancia o serviço com os mocks
            _service = new BusPublisherService(
                _serviceBusClientMock.Object,
                _loggerMock.Object,
                config
            );
        }

        // Teste: Verifica se publica na fila correta quando o tipo está mapeado
        [Fact]
        public async Task PublishAsync_DevePublicarNaFilaCorreta_QuandoTipoEstaMapeado()
        {
            // Arrange
            var message = new WebhookMetaInboundDTO("1", "payload", "signature");

            // Act
            await _service.PublishAsync(message);

            // Assert
            // Verifica se o CreateSender foi chamado com a fila correta
            _serviceBusClientMock.Verify(x => x.CreateSender("fila-webhook"), Times.Once);

            // Verifica se o SendMessageAsync foi chamado com uma mensagem corretamente serializada
            _serviceBusSenderMock.Verify(x =>
                x.SendMessageAsync(It.Is<ServiceBusMessage>(m =>
                    ValidarMensagemSerializada(m, message) // Função auxiliar para validar JSON
                ), default), Times.Once);
        }

        // Teste: Verifica se lança exceção e registra log se o envio falhar
        [Fact]
        public async Task PublishAsync_DeveLancarExcecaoELogarErro_QuandoSendFalhar()
        {
            // Arrange
            var message = new WebhookMetaInboundDTO("1", "payload", "signature");

            // Simula erro na hora de enviar a mensagem para o Service Bus
            _serviceBusSenderMock
                .Setup(x => x.SendMessageAsync(It.IsAny<ServiceBusMessage>(), default))
                .ThrowsAsync(new InfraException("Erro simulado"));

            // Act
            Func<Task> act = async () => await _service.PublishAsync(message);

            // Assert
            // Verifica se a exceção foi lançada
            await act.Should().ThrowAsync<InfraException>()
                .WithMessage("Erro ao publicar a mensagem no azure bus.");

            // Verifica se o erro foi registrado no log
            _loggerMock.Verify(
                l => l.Log(
                    LogLevel.Error,
                    It.IsAny<EventId>(),
                    It.Is<It.IsAnyType>((o, _) => o.ToString()!.Contains("Erro ao publicar mensagem")),
                    It.IsAny<Exception>(),
                    (Func<It.IsAnyType, Exception?, string>)It.IsAny<object>()),
                Times.Once);
        }

        // Função auxiliar para validar se a mensagem foi serializada corretamente
        private static bool ValidarMensagemSerializada(ServiceBusMessage message, WebhookMetaInboundDTO original)
        {
            var body = Encoding.UTF8.GetString(message.Body);
            var desserializado = JsonSerializer.Deserialize<WebhookMetaInboundDTO>(body);

            return desserializado != null &&
                   desserializado.IdExterno == original.IdExterno &&
                   desserializado.Payload == original.Payload &&
                   desserializado.AssinaturaHMAC == original.AssinaturaHMAC;
        }
    }
}
