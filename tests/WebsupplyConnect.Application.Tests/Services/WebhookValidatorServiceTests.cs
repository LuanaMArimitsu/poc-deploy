//using FluentAssertions;
//using Microsoft.Extensions.Logging;
//using Microsoft.Extensions.Options;
//using Moq;
//using WebsupplyConnect.Application.Configuration;
//using WebsupplyConnect.Application.Services.Comunicacao;
//using System.Text;

//namespace WebsupplyConnect.Application.Tests.Services
//{
//    public class WebhookValidatorServiceTests
//    {
//        private readonly WebhookValidatorService _service;
//        private readonly Mock<ILogger<WebhookValidatorService>> _loggerMock;

//        //public WebhookValidatorServiceTests()
//        //{
//        //    var options = Options.Create(new WebhookMetaConfig
//        //    {
//        //        WebhookSecret = "segredo-teste"
//        //    });

//        //    _loggerMock = new Mock<ILogger<WebhookValidatorService>>();

//        //    _service = new WebhookValidatorService(options, _loggerMock.Object);
//        //}

//        [Fact]
//        public void GenerateSignature_DeveGerarAssinaturaCorreta()
//        {
//            // Arrange - Preparação dos dados

//            // Simula um payload que seria recebido no webhook
//            var payload = "{\"teste\":123}";

//            // Gera manualmente a assinatura esperada com base no payload e no segredo configurado
//            var expectedSignature = GerarAssinaturaManual(payload, "segredo-teste");

//            // Act - Ação (executa o método que queremos testar)

//            // Chama o método GenerateSignature passando o payload
//            var signature = _service.GenerateSignature(payload);

//            // Assert - Verificação

//            // Valida se a assinatura gerada pelo método é exatamente igual à assinatura esperada
//            signature.Should().Be(expectedSignature);
//        }


//        [Fact]
//        public void IsValid_DeveRetornarTrue_QuandoAssinaturaCorreta()
//        {
//            // Arrange - Preparação dos dados

//            // Simula um payload
//            var payload = "{\"teste\":123}";

//            // Gera uma assinatura válida para esse payload utilizando o próprio serviço
//            // Obs.: O prefixo "sha256=" faz parte do padrão da assinatura HMAC usada pela Meta
//            var signature = "sha256=" + _service.GenerateSignature(payload);

//            // Act - Ação (executa o método que estamos testando)

//            // Verifica se a assinatura é considerada válida pelo método IsValid
//            var result = _service.IsValid(payload, signature);

//            // Assert - Verificação

//            // Espera que o resultado seja verdadeiro, já que a assinatura é válida
//            result.Should().BeTrue();
//        }


//        [Fact]
//        public void IsValid_DeveRetornarFalse_QuandoAssinaturaIncorreta()
//        {
//            // Arrange - Preparação dos dados

//            // Simula um payload
//            var payload = "{\"teste\":123}";

//            // Usa uma assinatura incorreta (errada propositalmente)
//            var signature = "sha256=assinaturaerrada";

//            // Act - Ação (executa o método que queremos testar)

//            // Verifica se a assinatura inválida é corretamente rejeitada
//            var result = _service.IsValid(payload, signature);

//            // Assert - Verificação

//            // Espera que o resultado seja falso, pois a assinatura é inválida
//            result.Should().BeFalse();
//        }


//        // Este método simula manualmente a geração da assinatura HMAC SHA256,
//        // exatamente da mesma forma que o método GenerateSignature faz.
//        // Serve para calcular a assinatura esperada no teste.
//        private static string GerarAssinaturaManual(string payload, string secret)
//        {
//            using var hmac = new System.Security.Cryptography.HMACSHA256(Encoding.UTF8.GetBytes(secret));
//            var hash = hmac.ComputeHash(Encoding.UTF8.GetBytes(payload));
//            return BitConverter.ToString(hash).Replace("-", "").ToLower();
//        }

//    }
//}
