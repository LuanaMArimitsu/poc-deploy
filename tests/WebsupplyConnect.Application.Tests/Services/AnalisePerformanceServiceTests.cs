//using Microsoft.Extensions.Logging;
//using Moq;
//using WebsupplyConnect.Application.DTOs.Distribuicao;
//using WebsupplyConnect.Application.DTOs.Usuario;
//using WebsupplyConnect.Application.Interfaces.Distribuicao;
//using WebsupplyConnect.Application.Interfaces.Lead;
//using WebsupplyConnect.Application.Interfaces.Usuario;
//using WebsupplyConnect.Application.Services.Distribuicao;
//using WebsupplyConnect.Domain.Entities.Usuario;

//namespace WebsupplyConnect.Application.Tests.Services
//{
//    public class AnalisePerformanceServiceTests
//    {
//        private readonly Mock<IDistribuicaoService> _mockDistribuicaoService;
//        private readonly Mock<IVendedorEstatisticasService> _mockEstatisticasService;
//        private readonly Mock<ILeadWriterService> _mockLeadService;
//        private readonly Mock<IUsuarioService> _mockUsuarioService;
//        private readonly Mock<ILogger<AnalisePerformanceService>> _mockLogger;
//        private readonly AnalisePerformanceService _service;

//        public AnalisePerformanceServiceTests()
//        {
//            _mockDistribuicaoService = new Mock<IDistribuicaoService>();
//            _mockEstatisticasService = new Mock<IVendedorEstatisticasService>();
//            _mockLeadService = new Mock<ILeadWriterService>();
//            _mockUsuarioService = new Mock<IUsuarioService>();
//            _mockLogger = new Mock<ILogger<AnalisePerformanceService>>();

//            _service = new AnalisePerformanceService(
//                _mockDistribuicaoService.Object,
//                _mockEstatisticasService.Object,
//                _mockLeadService.Object,
//                _mockUsuarioService.Object,
//                _mockLogger.Object);
//        }

//        [Fact]
//        public async Task AnalisarPerformanceAsync_ComParametrosValidos_RetornaResultadoCompleto()
//        {
//            // Arrange
//            var empresaId = 1;
//            var dataInicio = DateTime.UtcNow.AddDays(-30);
//            var dataFim = DateTime.UtcNow;

//            var vendedores = new List<UsuarioSimplesDTO>
//            {
//                new UsuarioSimplesDTO { Id = 1, Nome = "Vendedor 1" },
//                new UsuarioSimplesDTO { Id = 2, Nome = "Vendedor 2" }
//            };

//            _mockDistribuicaoService.Setup(x => x.CountHistoricoDistribuicaoAsync(empresaId, dataInicio, dataFim))
//                .ReturnsAsync(100);

//            _mockLeadService.Setup(x => x.CountLeadsDistribuidosAsync(empresaId, dataInicio, dataFim))
//                .ReturnsAsync(95);

//            _mockDistribuicaoService.Setup(x => x.GetTempoMedioDistribuicaoAsync(empresaId, dataInicio, dataFim))
//                .ReturnsAsync(5.5m);

//            _mockUsuarioService.Setup(x => x.UsuariosEmpresa(empresaId))
//                .ReturnsAsync(vendedores);

//            _mockEstatisticasService.Setup(x => x.CalcularTaxaConversaoAsync(1, empresaId, 30))
//                .ReturnsAsync(85.5m);

//            _mockEstatisticasService.Setup(x => x.CalcularTaxaConversaoAsync(2, empresaId, 30))
//                .ReturnsAsync(78.2m);

//            _mockEstatisticasService.Setup(x => x.CalcularVelocidadeMediaAtendimentoAsync(1, empresaId, 30))
//                .ReturnsAsync(3.2m);

//            _mockEstatisticasService.Setup(x => x.CalcularVelocidadeMediaAtendimentoAsync(2, empresaId, 30))
//                .ReturnsAsync(4.1m);

//            // Act
//            var resultado = await _service.AnalisarPerformanceAsync(
//                empresaId, 
//                dataInicio, 
//                dataFim, 
//                incluirMetricasVendedor: true, 
//                incluirTendencias: false);

//            // Assert
//            Assert.NotNull(resultado);
//            Assert.Equal(empresaId, resultado.EmpresaId);
//            Assert.Equal(dataInicio, resultado.DataInicio);
//            Assert.Equal(dataFim, resultado.DataFim);

//            // Verificar métricas gerais
//            Assert.NotNull(resultado.MetricasGerais);
//            Assert.Equal(100, resultado.MetricasGerais.TotalDistribuicoes);
//            Assert.Equal(95, resultado.MetricasGerais.TotalLeadsDistribuidos);
//            Assert.Equal(5.5m, resultado.MetricasGerais.TempoMedioDistribuicaoSegundos);
//            Assert.Equal(2, resultado.MetricasGerais.VendedoresAtivos);
//            Assert.Equal(95.0m, resultado.MetricasGerais.TaxaSucesso);
//            Assert.Equal(47.5m, resultado.MetricasGerais.LeadsPorVendedor);

//            // Verificar métricas por vendedor
//            Assert.NotNull(resultado.MetricasVendedores);
//            Assert.Equal(2, resultado.MetricasVendedores.Count);

//            var primeiroVendedor = resultado.MetricasVendedores.First();
//            Assert.Equal(1, primeiroVendedor.VendedorId);
//            Assert.Equal("Vendedor 1", primeiroVendedor.NomeVendedor);
//            Assert.Equal(85.5m, primeiroVendedor.TaxaConversao);
//            Assert.Equal(3.2m, primeiroVendedor.VelocidadeMediaAtendimento);
//            Assert.True(primeiroVendedor.ScorePerformance > 0);
//            Assert.NotEmpty(primeiroVendedor.NivelPerformance);
//        }

//        [Fact]
//        public async Task AnalisarPerformanceAsync_SemMetricasVendedor_RetornaResultadoSemMetricasVendedor()
//        {
//            // Arrange
//            var empresaId = 1;
//            var dataInicio = DateTime.UtcNow.AddDays(-30);
//            var dataFim = DateTime.UtcNow;

//            _mockDistribuicaoService.Setup(x => x.CountHistoricoDistribuicaoAsync(empresaId, dataInicio, dataFim))
//                .ReturnsAsync(50);

//            _mockLeadService.Setup(x => x.CountLeadsDistribuidosAsync(empresaId, dataInicio, dataFim))
//                .ReturnsAsync(45);

//            _mockDistribuicaoService.Setup(x => x.GetTempoMedioDistribuicaoAsync(empresaId, dataInicio, dataFim))
//                .ReturnsAsync(3.2m);

//            _mockUsuarioService.Setup(x => x.UsuariosEmpresa(empresaId))
//                .ReturnsAsync(new List<UsuarioSimplesDTO>());

//            // Act
//            var resultado = await _service.AnalisarPerformanceAsync(
//                empresaId, 
//                dataInicio, 
//                dataFim, 
//                incluirMetricasVendedor: false, 
//                incluirTendencias: false);

//            // Assert
//            Assert.NotNull(resultado);
//            Assert.Equal(empresaId, resultado.EmpresaId);
//            Assert.NotNull(resultado.MetricasGerais);
//            Assert.Null(resultado.MetricasVendedores);
//            Assert.Null(resultado.Tendencia);
//        }

//        [Fact]
//        public async Task AnalisarPerformanceAsync_ComTendencias_RetornaResultadoComTendencias()
//        {
//            // Arrange
//            var empresaId = 1;
//            var dataInicio = DateTime.UtcNow.AddDays(-30);
//            var dataFim = DateTime.UtcNow;

//            _mockDistribuicaoService.Setup(x => x.CountHistoricoDistribuicaoAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
//                .ReturnsAsync(100);

//            _mockLeadService.Setup(x => x.CountLeadsDistribuidosAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
//                .ReturnsAsync(95);

//            _mockDistribuicaoService.Setup(x => x.GetTempoMedioDistribuicaoAsync(It.IsAny<int>(), It.IsAny<DateTime?>(), It.IsAny<DateTime?>()))
//                .ReturnsAsync(5.5m);

//            _mockUsuarioService.Setup(x => x.UsuariosEmpresa(empresaId))
//                .ReturnsAsync(new List<UsuarioSimplesDTO>());

//            // Act
//            var resultado = await _service.AnalisarPerformanceAsync(
//                empresaId, 
//                dataInicio, 
//                dataFim, 
//                incluirMetricasVendedor: false, 
//                incluirTendencias: true,
//                periodoTendenciasDias: 30);

//            // Assert
//            Assert.NotNull(resultado);
//            Assert.NotNull(resultado.Tendencia);
//            Assert.Equal(30, resultado.Tendencia.PeriodoDias);
//            Assert.NotEmpty(resultado.Tendencia.TendenciaConversao);
//            Assert.NotEmpty(resultado.Tendencia.TendenciaVelocidade);
//            Assert.NotNull(resultado.Tendencia.Recomendacoes);
//        }

//        [Fact]
//        public async Task AnalisarPerformanceAsync_ComErro_PropagaExcecao()
//        {
//            // Arrange
//            var empresaId = 1;
//            var dataInicio = DateTime.UtcNow.AddDays(-30);
//            var dataFim = DateTime.UtcNow;

//            _mockDistribuicaoService.Setup(x => x.CountHistoricoDistribuicaoAsync(empresaId, dataInicio, dataFim))
//                .ThrowsAsync(new InvalidOperationException("Erro de teste"));

//            // Act & Assert
//            await Assert.ThrowsAsync<InvalidOperationException>(() =>
//                _service.AnalisarPerformanceAsync(empresaId, dataInicio, dataFim));
//        }

//        [Fact]
//        public async Task AnalisarPerformanceAsync_SemParametros_UsaValoresPadrao()
//        {
//            // Arrange
//            var empresaId = 1;

//            _mockDistribuicaoService.Setup(x => x.CountHistoricoDistribuicaoAsync(empresaId, null, null))
//                .ReturnsAsync(100);

//            _mockLeadService.Setup(x => x.CountLeadsDistribuidosAsync(empresaId, null, null))
//                .ReturnsAsync(95);

//            _mockDistribuicaoService.Setup(x => x.GetTempoMedioDistribuicaoAsync(empresaId, null, null))
//                .ReturnsAsync(5.5m);

//            _mockUsuarioService.Setup(x => x.UsuariosEmpresa(empresaId))
//                .ReturnsAsync(new List<UsuarioSimplesDTO>());

//            // Act
//            var resultado = await _service.AnalisarPerformanceAsync(empresaId);

//            // Assert
//            Assert.NotNull(resultado);
//            Assert.Equal(empresaId, resultado.EmpresaId);
//            Assert.Null(resultado.DataInicio);
//            Assert.Null(resultado.DataFim);
//            Assert.NotNull(resultado.MetricasGerais);
//            Assert.NotNull(resultado.MetricasVendedores);
//            Assert.Null(resultado.Tendencia);
//        }
//    }
//}
