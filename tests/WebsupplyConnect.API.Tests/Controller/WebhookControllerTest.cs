//using FluentAssertions;
//using Microsoft.AspNetCore.Http;
//using Microsoft.AspNetCore.Mvc;
//using Microsoft.Extensions.Configuration;
//using Microsoft.Extensions.Logging;
//using Moq;
//using WebsupplyConnect.API.Response;
//using WebsupplyConnect.Application.Interfaces.Comunicacao;
//using System.Text;
//using Xunit;
//using WebsupplyConnect.API.Controllers.Comunicacao;

//namespace WebsupplyConnect.API.Tests.Controller
//{
//    public class WebhookControllerTest
//    {
//        private readonly Mock<IWebhookValidatorService> _validatorMock;
//        private readonly Mock<IWebhookProcessorService> _processorMock;
//        private readonly Mock<ILogger<WebhookController>> _loggerMock;
//        private readonly IConfiguration _configuration;
//        private readonly WebhookController _controller;

//        public WebhookControllerTest()
//        {
//            _validatorMock = new Mock<IWebhookValidatorService>();
//            _processorMock = new Mock<IWebhookProcessorService>();
//            _loggerMock = new Mock<ILogger<WebhookController>>();

//            var inMemorySettings = new Dictionary<string, string?>
//            {
//                { "WhatsApp:VerifyToken", "token-teste" }
//            };
//            _configuration = new ConfigurationBuilder()
//                .AddInMemoryCollection(inMemorySettings)
//                .Build();

//            _controller = new WebhookController(
//                _validatorMock.Object,
//                _processorMock.Object,
//                _loggerMock.Object,
//                _configuration
//            );
//        }

//        [Fact]
//        public void VerifyWebhook_DeveRetornarOk_QuandoTokenValido()
//        {
//            var mode = "subscribe";
//            var token = "token-teste";
//            var challenge = "desafio-teste";

//            var result = _controller.VerifyWebhook(mode, token, challenge);

//            result.Should().BeOfType<OkObjectResult>();
//            var okResult = result as OkObjectResult;
//            okResult!.Value.Should().Be(challenge);
//        }

//        [Fact]
//        public void VerifyWebhook_DeveRetornarUnauthorized_QuandoTokenInvalido()
//        {
//            var mode = "subscribe";
//            var token = "token-invalido";
//            var challenge = "desafio";

//            var result = _controller.VerifyWebhook(mode, token, challenge);
//            result.Should().BeOfType<UnauthorizedObjectResult>();

//            var unauthorized = result as UnauthorizedObjectResult;
//            var response = unauthorized!.Value.Should().BeOfType<ApiResponse<string>>().Subject;
//            response.Success.Should().BeFalse();
//            response.Message.Should().Be("Verificação falhou.");

//        }

//        [Fact]
//        public async Task ProcessWebhook_DeveRetornarUnauthorized_QuandoAssinaturaAusente()
//        {
//            ConfigurarHttpContext();

//            var json = "{}";
//            SetRequestBody(json);

//            var result = await _controller.ProcessWebhook();
//            result.Should().BeOfType<UnauthorizedObjectResult>();

//            var unauthorized = result as UnauthorizedObjectResult;

//            // Verifica se o conteúdo é do tipo ApiResponse<string>
//            var response = unauthorized!.Value.Should().BeOfType<ApiResponse<string>>().Subject;

//            response.Success.Should().BeFalse();
//            response.Message.Should().Be("Assinatura ausente.");
//            response.Data.Should().BeNull(); // opcional, se quiser testar também
//        }

//        [Fact]
//        public async Task ProcessWebhook_DeveRetornarUnauthorized_QuandoAssinaturaInvalida()
//        {
//            ConfigurarHttpContext();

//            var json = "{ \"teste\": 1 }";
//            SetRequestBody(json);
//            SetRequestHeader("X-Hub-Signature-256", "assinatura-invalida");

//            _validatorMock.Setup(v => v.IsValid(json, "assinatura-invalida"))
//                .Returns(false);

//            var result = await _controller.ProcessWebhook();

//            result.Should().BeOfType<UnauthorizedObjectResult>();
//            var unauthorized = result as UnauthorizedObjectResult;
//            var response = unauthorized!.Value.Should().BeOfType<ApiResponse<string>>().Subject;
//            response.Message.Should().Be("Assinatura inválida.");
//        }

//        [Fact]
//        public async Task ProcessWebhook_DeveRetornarOk_QuandoAssinaturaValida()
//        {
//            ConfigurarHttpContext();

//            var json = "{ \"teste\": 1 }";
//            SetRequestBody(json);
//            SetRequestHeader("X-Hub-Signature-256", "assinatura-valida");

//            _validatorMock.Setup(v => v.IsValid(json, "assinatura-valida"))
//                .Returns(true);

//            var result = await _controller.ProcessWebhook();

//            result.Should().BeOfType<OkResult>();
//        }

//        private void ConfigurarHttpContext()
//        {
//            _controller.ControllerContext = new ControllerContext();
//            _controller.ControllerContext.HttpContext = new DefaultHttpContext();
//        }

//        private void SetRequestBody(string json)
//        {
//            var bodyStream = new MemoryStream(Encoding.UTF8.GetBytes(json));
//            _controller.ControllerContext.HttpContext.Request.Body = bodyStream;
//        }

//        private void SetRequestHeader(string headerName, string headerValue)
//        {
//            _controller.ControllerContext.HttpContext.Request.Headers[headerName] = headerValue;
//        }
//    }
//}
