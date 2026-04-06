//using FluentAssertions;
//using Microsoft.Extensions.Logging;
//using Moq;
//using WebsupplyConnect.Application.Common;
//using WebsupplyConnect.Application.DTOs.ExternalServices;
//using WebsupplyConnect.Application.Interfaces.Comunicacao;
//using WebsupplyConnect.Application.Interfaces.ExternalServices;
//using WebsupplyConnect.Application.Services.Comunicacao;

//namespace WebsupplyConnect.Application.Tests.Services
//{
//    public class WebhookProcessorServiceTests
//    {
//        private readonly Mock<IBusPublisherService> _busPublisherMock;
//        private readonly Mock<IWebhookValidatorService> _webhookValidatorMock;
//        private readonly Mock<ILogger<WebhookProcessorService>> _loggerMock;
//        private readonly WebhookProcessorService _service;

//        public WebhookProcessorServiceTests()
//        {
//            _busPublisherMock = new Mock<IBusPublisherService>();
//            _webhookValidatorMock = new Mock<IWebhookValidatorService>();
//            _loggerMock = new Mock<ILogger<WebhookProcessorService>>();
//            _service = new WebhookProcessorService(
//                _busPublisherMock.Object,
//                _loggerMock.Object,
//                _webhookValidatorMock.Object
//            );
//        }

//        [Fact]
//        public async Task ProcessWebhookAsync_DevePublicarMensagemComAssinaturaCorreta()
//        {
//            // Arrange - Preparação dos dados e configuração dos mocks

//            // Simula o payload recebido pelo webhook (poderia ser qualquer string JSON)
//            var payload = "{ \"message\": \"teste\" }";

//            // Simula a assinatura gerada para esse payload
//            var expectedSignature = "assinaturaFake";

//            // Configura o mock do validador de webhook para, ao receber o payload,
//            // retornar a assinatura esperada (assinaturaFake)
//            _webhookValidatorMock
//                .Setup(v => v.GenerateSignature(payload))
//                .Returns(expectedSignature);

//            // Act - Ação (execução do método que será testado)

//            // Chama o método 'ProcessWebhookAsync' passando o payload simulado
//            await _service.ProcessWebhookAsync(payload);

//            // Assert - Verificação dos comportamentos

//            // Verifica se o método 'PublishAsync' do serviço de publicação foi chamado
//            // exatamente uma vez e se os dados enviados são os corretos:
//            // - O payload deve ser igual ao payload simulado
//            // - A assinatura deve ser igual à assinatura gerada
//            // - O IdExterno não pode ser nulo ou vazio (é gerado dentro do método)
//            _busPublisherMock.Verify(p => p.PublishAsync(It.Is<WebhookMetaInboundDTO>(dto =>
//                dto.Payload == payload &&
//                dto.AssinaturaHMAC == expectedSignature &&
//                !string.IsNullOrEmpty(dto.IdExterno)
//            )), Times.Once);

//            // Além disso, verifica se o método de geração da assinatura foi chamado exatamente uma vez
//            _webhookValidatorMock.Verify(v => v.GenerateSignature(payload), Times.Once);
//        }
     
//        [Fact]
//        public async Task ProcessWebhookAsync_QuandoPublicacaoFalhar_DeveLancarExcecao()
//        {
//            // Arrange - Preparação dos dados e configuração dos mocks

//            // Simula um payload recebido pelo webhook
//            var payload = "{ \"message\": \"teste\" }";

//            // Simula a assinatura gerada para esse payload
//            var expectedSignature = "assinaturaFake";

//            // Configura o mock do validador para, ao receber o payload, 
//            // retornar a assinatura simulada (assinaturaFake)
//            _webhookValidatorMock
//                .Setup(v => v.GenerateSignature(payload))
//                .Returns(expectedSignature);

//            // Configura o mock do serviço de publicação para que, ao tentar publicar qualquer WebhookMetaDTO,
//            // lance uma exceção do tipo InvalidOperationException, simulando uma falha na publicação na fila
//            _busPublisherMock
//                .Setup(p => p.PublishAsync(It.IsAny<WebhookMetaInboundDTO>()))
//                .ThrowsAsync(new AppException("Erro na fila"));

//            // Act - Ação (execução do método que está sendo testado)

//            // Define uma função assíncrona 'act' que executa o método que será testado.
//            // Essa abordagem permite que a exceção lançada seja capturada e validada no Assert
//            Func<Task> act = async () => await _service.ProcessWebhookAsync(payload);

//            // Assert - Verificação dos comportamentos

//            // Verifica se ao executar o método, ele lança uma exceção do tipo InvalidOperationException,
//            // e se a mensagem da exceção é "Erro na fila"
//            await act.Should()
//                .ThrowAsync<AppException>()
//                .WithMessage("Error ao processar webhook");

//            // Verifica que o método de geração da assinatura foi chamado exatamente uma vez
//            _webhookValidatorMock.Verify(v => v.GenerateSignature(payload), Times.Once);

//            // Verifica que o método de publicação também foi chamado exatamente uma vez,
//            // mesmo que tenha resultado em erro
//            _busPublisherMock.Verify(p => p.PublishAsync(It.IsAny<WebhookMetaInboundDTO>()), Times.Once);
//        }

//    }
//}
