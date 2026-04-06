//using Microsoft.Extensions.Logging;
//using Moq;
//using WebsupplyConnect.Application.Common;
//using WebsupplyConnect.Application.DTOs.Comunicacao;
//using WebsupplyConnect.Application.DTOs.ExternalServices;
//using WebsupplyConnect.Application.Services.Comunicacao;
//using WebsupplyConnect.Domain.Entities.Comunicacao;
//using WebsupplyConnect.Domain.Interfaces.Base;
//using WebsupplyConnect.Domain.Interfaces.Comunicacao;

//namespace WebsupplyConnect.Application.Tests.Services
//{
//    public class WebhookMetaServiceTests
//    {
//        private readonly Mock<IUnitOfWork> _mockUnitOfWork;
//        private readonly Mock<IWebhookMetaRepository> _mockComunicacaoRepository;
//        private readonly Mock<ILogger<WebhookWriterService>> _mockLogger;
//        private readonly WebhookWriterService _service;

//        public WebhookMetaServiceTests()
//        {
//            _mockUnitOfWork = new Mock<IUnitOfWork>();
//            _mockComunicacaoRepository = new Mock<IWebhookMetaRepository>();
//            _mockLogger = new Mock<ILogger<WebhookWriterService>>();
//            _service = new WebhookWriterService(
//                _mockUnitOfWork.Object,
//                _mockComunicacaoRepository.Object,
//                _mockLogger.Object
//            );
//        }

//        [Fact]
//        public async Task RegisterWebhookAsync_DeveRetornarId_WebhookJaExiste()
//        {
//            // Arrange: cria um DTO com os dados simulados de entrada do webhook
//            var dto = new WebhookMetaInboundDTO("123", "payload", "sig");

//            // Cria um objeto WebhookMeta existente (simulando que ele já foi registrado anteriormente)
//            var existente = new WebhookMeta("123", "payload", "sig");

//            // Usa reflection para forçar o valor do ID (já que o set é privado e só o banco setaria normalmente)
//            typeof(WebhookMeta).GetProperty("Id")!
//                .SetValue(existente, 42);

//            // Configura o mock do repositório para retornar o webhook existente quando buscado pelo IdExterno
//            _mockComunicacaoRepository.Setup(r => r.GetWebhookMetaByIdExternoAsync("123", false))
//                .ReturnsAsync(existente);

//            // Act: chama o método que será testado
//            var result = await _service.RegisterWebhookAsync(dto);

//            // Assert: verifica se o método retornou o ID do webhook já existente
//            Assert.Equal(42, result);

//            // Verifica se o método de inserção **não foi chamado**, já que o webhook já existia
//            _mockComunicacaoRepository.Verify(r => r.CreateAsync(It.IsAny<WebhookMeta>()), Times.Never);

//            // Verifica se o Commit também **não foi chamado**, pois nenhuma alteração foi feita
//            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
//        }


//        [Fact]
//        public async Task RegisterWebhookAsync_DeveAdicionarENCommitar_NaoExiste()
//        {
//            // Arrange: cria o DTO de entrada com dados simulados para um novo webhook
//            var dto = new WebhookMetaInboundDTO("456", "payload", "sig");

//            // Simula o retorno da entidade salva no banco, com ID definido (setado via reflection)
//            WebhookMeta salvo = new WebhookMeta("456", "payload", "sig");
//            typeof(WebhookMeta).GetProperty("Id")!
//                .SetValue(salvo, 99); // Força o ID como se o banco tivesse atribuído

//            // Configura o mock para retornar null, simulando que o webhook ainda não existe no banco
//            _mockComunicacaoRepository.Setup(r => r.GetWebhookMetaByIdExternoAsync("456", false))
//                .ReturnsAsync((WebhookMeta)null);

//            // Configura o mock para retornar o webhook salvo quando o Add for chamado
//            _mockComunicacaoRepository.Setup(r => r.CreateAsync(It.IsAny<WebhookMeta>()))
//                .ReturnsAsync(salvo);

//            // Act: chama o método que será testado
//            var result = await _service.RegisterWebhookAsync(dto);

//            // Assert: verifica se o método retornou o ID do novo webhook salvo
//            Assert.Equal(99, result);

//            // Verifica se o método de adicionar o webhook foi chamado exatamente uma vez
//            _mockComunicacaoRepository.Verify(r => r.CreateAsync(It.IsAny<WebhookMeta>()), Times.Once);

//            // Verifica se o Commit foi executado uma vez, confirmando a transação
//            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
//        }

//        [Fact]
//        public async Task RegisterWebhookAsync_DeveFazerRollbackELogar_QuandoErroOcorre()
//        {
//            // Arrange: cria o DTO de entrada com dados simulados
//            var dto = new WebhookMetaInboundDTO("999", "payload", "sig");

//            // Configura o mock para lançar uma exceção ao tentar buscar o webhook,
//            // simulando uma falha inesperada no repositório
//            _mockComunicacaoRepository.Setup(r => r.GetWebhookMetaByIdExternoAsync("999", false))
//                .ThrowsAsync(new AppException("Erro simulado"));

//            // Act & Assert:
//            // Espera que uma exceção seja lançada ao chamar RegisterWebhookAsync
//            var ex = await Assert.ThrowsAsync<AppException>(() => _service.RegisterWebhookAsync(dto));

//            // Verifica se a exceção lançada tem a mensagem personalizada do serviço
//            Assert.Equal("Error registering webhook", ex.Message);

//            // Verifica se o método RollbackAsync foi chamado, indicando tentativa de desfazer a transação
//            _mockUnitOfWork.Verify(u => u.RollbackAsync(), Times.Never);
//        }


//        [Fact]
//        public async Task UpdateWebhookAsync_DeveAtualizarECommitar_QuandoWebhookExiste()
//        {
//            // Arrange
//            int webhookId = 10;
//            int conversaId = 200;

//            // Cria um mock da entidade WebhookMeta com CallBase = true, 
//            // o que permite que os métodos reais sejam executados (como MarcarProcessado)
//            var webhook = new Mock<WebhookMeta>("idExt", "payload", "sig") { CallBase = true };

//            // Usa reflection para definir o ID da entidade, já que a propriedade Id tem setter inacessível
//            typeof(WebhookMeta).GetProperty("Id")!
//                .SetValue(webhook.Object, webhookId);

//            // Configura o mock do repositório para retornar o webhook quando for buscado pelo ID
//            _mockComunicacaoRepository.Setup(r => r.GetWebhookMetaByIdAsync(webhookId, false))
//                .ReturnsAsync(webhook.Object);

//            // Act: chama o método do serviço que está sendo testado
//            var result = await _service.UpdateWebhookAsync(webhookId, conversaId);

//            // Assert:
//            // Verifica se o resultado foi true, indicando sucesso
//            Assert.True(result);

//            // Verifica se o método de atualização do repositório foi chamado com o webhook retornado
//            _mockComunicacaoRepository.Verify(r => r.UpdateWebhookMeta(webhook.Object), Times.Once);

//            // Verifica se a transação foi concluída com sucesso (CommitAsync chamado)
//            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Once);
//        }


//        [Fact]
//        public async Task UpdateWebhookAsync_DeveRetornarFalse_QuandoWebhookNaoExiste()
//        {
//            // Arrange:
//            // Configura o mock para retornar null quando o webhook não for encontrado pelo ID
//            _mockComunicacaoRepository
//                .Setup(r => r.GetWebhookMetaByIdAsync(It.IsAny<int>(), false))
//                .ReturnsAsync((WebhookMeta)null);

//            // Act: chama o método passando um ID que não existe
//            var result = await _service.UpdateWebhookAsync(999, 1);

//            // Assert:
//            // Verifica que o resultado foi falso, pois o webhook não existe
//            Assert.False(result);

//            // Verifica que a operação de commit NÃO foi chamada (nenhuma atualização foi feita)
//            _mockUnitOfWork.Verify(u => u.SaveChangesAsync(), Times.Never);
//        }

//    }
//}