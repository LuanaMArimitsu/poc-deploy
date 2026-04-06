//using Microsoft.Extensions.Logging;
//using Moq;
//using WebsupplyConnect.Application.Common;
//using WebsupplyConnect.Application.Interfaces.Distribuicao;
//using WebsupplyConnect.Application.Interfaces.Lead;
//using WebsupplyConnect.Application.Interfaces.Usuario;
//using WebsupplyConnect.Application.Services.Distribuicao;
//using WebsupplyConnect.Domain.Interfaces.Distribuicao;

//namespace WebsupplyConnect.Application.Tests.Services
//{
//    public class DistribuicaoServiceTests
//    {
//        private readonly Mock<IDistribuicaoRepository> _distribuicaoRepositoryMock;
//        private readonly Mock<IAtribuicaoLeadService> _atribuicaoLeadServiceMock;
//        private readonly Mock<ILeadReaderService> _leadReaderServiceMock;
//        private readonly Mock<ILeadResponsavelWriter> _leadResponsavelWriter;
//        private readonly Mock<IUsuarioService> _usuarioServiceMock;
//        private readonly Mock<IDistribuicaoConfigurationService> _configurationServiceMock;
//        private readonly Mock<IDistribuicaoContextService> _contextServiceMock;
//        private readonly Mock<IRegraDistribuicaoProvider> _regraDistribuicaoProviderMock;
//        private readonly Mock<IMetricaVendedorService> _metricaServiceMock;
//        private readonly Mock<IFilaDistribuicaoService> _filaServiceMock;
//        private readonly Mock<IScoreCalculationService> _scoreCalculationServiceMock;
//        private readonly Mock<ILogger<DistribuicaoService>> _loggerMock;
//        private readonly DistribuicaoService _distribuicaoService;

//        public DistribuicaoServiceTests()
//        {
//            _distribuicaoRepositoryMock = new Mock<IDistribuicaoRepository>();
//            _atribuicaoLeadServiceMock = new Mock<IAtribuicaoLeadService>();
//            _leadReaderServiceMock = new Mock<ILeadReaderService>();
//            _leadResponsavelWriter = new Mock<ILeadResponsavelWriter>();
//            _usuarioServiceMock = new Mock<IUsuarioService>();
//            _configurationServiceMock = new Mock<IDistribuicaoConfigurationService>();
//            _contextServiceMock = new Mock<IDistribuicaoContextService>();
//            _regraDistribuicaoProviderMock = new Mock<IRegraDistribuicaoProvider>();
//            _metricaServiceMock = new Mock<IMetricaVendedorService>();
//            _filaServiceMock = new Mock<IFilaDistribuicaoService>();
//            _scoreCalculationServiceMock = new Mock<IScoreCalculationService>();
//            _loggerMock = new Mock<ILogger<DistribuicaoService>>();

//            _distribuicaoService = new DistribuicaoService(
//                _distribuicaoRepositoryMock.Object,
//                _atribuicaoLeadServiceMock.Object,
//                _usuarioServiceMock.Object,
//                _leadReaderServiceMock.Object,
//                _leadResponsavelWriter.Object,
//                _configurationServiceMock.Object,
//                _contextServiceMock.Object,
//                _regraDistribuicaoProviderMock.Object,
//                _metricaServiceMock.Object,
//                _filaServiceMock.Object,
//                _scoreCalculationServiceMock.Object,
//                _loggerMock.Object
//            );
//        }

//        /// <summary>
//        /// Testa o cenário onde o leadId é inválido (menor ou igual a zero)
//        /// </summary>
//        [Fact]
//        public async Task AtribuirResponsavelParaLeadAsync_LeadIdInvalido_DeveLancarExcecao()
//        {
//            // Arrange
//            var leadId = 0;

//            // Act & Assert
//            var exception = await Assert.ThrowsAsync<ApplicationException>(
//                () => _distribuicaoService.AtribuirResponsavelParaLeadAsync(leadId));

//            Assert.Contains("ID do lead deve ser maior que zero", exception.Message);
//        }

//        /// <summary>
//        /// Testa o cenário onde o lead não é encontrado
//        /// </summary>
//        [Fact]
//        public async Task AtribuirResponsavelParaLeadAsync_LeadNaoEncontrado_DeveLancarExcecao()
//        {
//            // Arrange
//            var leadId = 123;
//            _leadReaderServiceMock
//              .Setup(x => x.GetLeadByIdAsync(leadId))
//              .ThrowsAsync(new AppException("Lead não encontrado"));

//            // Act & Assert
//            var exception = await Assert.ThrowsAsync<ApplicationException>(
//                () => _distribuicaoService.AtribuirResponsavelParaLeadAsync(leadId));

//            Assert.Contains("Lead não encontrado", exception.Message);
//        }



//        /// <summary>
//        /// Testa o cenário onde a lead não é encontrada
//        /// </summary>
//        [Fact]
//        public async Task AtribuirResponsavelParaLeadAsync_LeadNaoEncontrada_DeveLancarExcecao()
//        {
//            // Arrange
//            var leadId = 999;

//            _leadReaderServiceMock
//           .Setup(x => x.GetLeadByIdAsync(leadId))
//           .ThrowsAsync(new AppException("Lead não encontrado"));


//            // Act & Assert
//            var exception = await Assert.ThrowsAsync<ApplicationException>(
//                () => _distribuicaoService.AtribuirResponsavelParaLeadAsync(leadId));

//            Assert.Contains("Lead não encontrado", exception.Message);
//        }


//    }
//}
