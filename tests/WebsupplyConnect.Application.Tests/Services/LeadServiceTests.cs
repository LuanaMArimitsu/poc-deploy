////using Microsoft.Extensions.Logging;
////using Moq;
////using WebsupplyConnect.Application.Common;
////using WebsupplyConnect.Application.DTOs.Redis;
////using WebsupplyConnect.Application.Interfaces.Comunicacao;
////using WebsupplyConnect.Application.Interfaces.ExternalServices;
////using WebsupplyConnect.Application.Interfaces.Lead;
////using WebsupplyConnect.Application.Interfaces.Usuario;
////using WebsupplyConnect.Application.Services.Lead;
////using WebsupplyConnect.Domain.Entities.Comunicacao;
////using WebsupplyConnect.Domain.Interfaces.Base;
////using WebsupplyConnect.Domain.Interfaces.Lead;

////namespace WebsupplyConnect.Application.Tests.Services
////{
////    public class LeadServiceTests
////    {
////        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
////        private readonly Mock<ILeadRepository> _leadRepositoryMock;
////        private readonly Mock<ILogger<LeadService>> _loggerMock;
////        private readonly Mock<IRedisCacheService> _redisCacheServiceMock;
////        private readonly Mock<ICanalService> _canalServiceMock;
////        private readonly Mock<IEnderecoService> _enderecoServiceMock;
////        private readonly Mock<IUsuarioService> _usuarioServiceMock;
////        private readonly LeadService _leadService;

////        //public LeadServiceTests()
////        //{
////        //    _unitOfWorkMock = new Mock<IUnitOfWork>();
////        //    _leadRepositoryMock = new Mock<ILeadRepository>();
////        //    _loggerMock = new Mock<ILogger<LeadService>>();
////        //    _redisCacheServiceMock = new Mock<IRedisCacheService>();
////        //    _canalServiceMock = new Mock<ICanalService>();
////        //    _enderecoServiceMock = new Mock<IEnderecoService>();
////        //    _usuarioServiceMock = new Mock<IUsuarioService>();

////        //    _leadService = new LeadService(
////        //        _unitOfWorkMock.Object,
////        //        _leadRepositoryMock.Object,
////        //        _loggerMock.Object,
////        //        _redisCacheServiceMock.Object,
////        //        _canalServiceMock.Object,
////        //        _enderecoServiceMock.Object,
////        //        _usuarioServiceMock.Object
////        //    );
////        //}

////        /// <summary>
////        /// Testa o cenário onde o lead existe no cache Redis.
////        /// Verifica se o lead é retornado do cache sem consultar o banco de dados.
////        /// </summary>
////        [Fact]
////        public async Task VerificarLeadExistente_LeadNoCache_DeveRetornarDoCache()
////        {
////            // Arrange - Preparação
////            var whatsappNumero = "11999887766";
////            var canalId = 1;
////            var whatsappNormalizado = "5511999887766";
////            var leadCache = new LeadRedisDTO(1, "Lead Teste", whatsappNormalizado, 10, 31);

////            _redisCacheServiceMock.Setup(x => x.GetAsync<LeadRedisDTO>($"lead:whatsapp:{whatsappNormalizado}:canal:{canalId}"))
////                .ReturnsAsync(leadCache);

////            // Act - Ação
////            var resultado = await _leadService.VerificarLeadExistente(whatsappNumero, canalId);

////            // Assert - Verificação
////            Assert.NotNull(resultado);
////            Assert.Equal(leadCache.Id, resultado.leadId);
////            Assert.Equal(leadCache.ResponsavelId, resultado.responsavelId);
//            // Assert - Verificação
//            Assert.NotNull(resultado);
//            Assert.Equal(leadCache.Id, resultado.LeadId);
//            Assert.Equal(leadCache.ResponsavelId, resultado.ResponsavelId);

////            _redisCacheServiceMock.Verify(x => x.GetAsync<LeadRedisDTO>($"lead:whatsapp:{whatsappNormalizado}:canal:{canalId}"), Times.Once);
////            _leadRepositoryMock.Verify(x => x.GetLeadByWhatsAppNumberAsync(It.IsAny<string>(), It.IsAny, Times.Never);
////        }

////        /// <summary>
////        /// Testa o cenário onde o lead não está no cache mas existe no banco de dados.
////        /// Verifica se o lead é buscado no banco e posteriormente armazenado no cache.
////        /// </summary>
////        [Fact]
////        public async Task VerificarLeadExistente_LeadNaoNoCacheMasNoBanco_DeveRetornarDoBancoEArmazenarNoCache()
////        {
////            // Arrange - Preparação
////            var whatsappNumero = "11999887766";
////            var canalId = 1;
////            var whatsappNormalizado = "5511999887766";
////            var leadBanco = new Domain.Entities.Lead.Lead(whatsappNormalizado, 1, 10, 1, 31);
////            leadBanco.GetType().GetProperty("Id")?.SetValue(leadBanco, 1);
////            leadBanco.GetType().GetProperty("Nome")?.SetValue(leadBanco, "Lead Teste");

////            _redisCacheServiceMock.Setup(x => x.GetAsync<LeadRedisDTO>($"lead:whatsapp:{whatsappNormalizado}:canal:{canalId}"))
////                .ReturnsAsync((LeadRedisDTO)null);

////            _leadRepositoryMock.Setup(x => x.GetLeadByWhatsAppNumberAsync(whatsappNormalizado, canalId))
////                .ReturnsAsync(leadBanco);

////            // Act - Ação
////            var resultado = await _leadService.VerificarLeadExistente(whatsappNumero, canalId);

//            // Assert - Verificação
//            Assert.NotNull(resultado);
//            Assert.Equal(leadBanco.Id, resultado.LeadId);
//            Assert.Equal(leadBanco.ResponsavelId, resultado.ResponsavelId);

////            _redisCacheServiceMock.Verify(x => x.GetAsync<LeadRedisDTO>($"lead:whatsapp:{whatsappNormalizado}:canal:{canalId}"), Times.Once);
////            _leadRepositoryMock.Verify(x => x.GetLeadByWhatsAppNumberAsync(whatsappNormalizado, canalId), Times.Once);

////            _redisCacheServiceMock.Verify(x => x.SetAsync(
////               $"lead:whatsapp:{whatsappNormalizado}:canal:{canalId}",
////               It.IsAny<LeadRedisDTO>(),
////               It.IsAny<TimeSpan?>()), Times.Once);
////        }

////        /// <summary>
////        /// Testa o cenário onde o lead não existe e precisa ser criado automaticamente.
////        /// Verifica se um novo lead é criado com todas as validações e dependências.
////        /// </summary>
////        [Fact]
////        public async Task VerificarLeadExistente_LeadNaoExiste_DeveCriarNovoLead()
////        {
////            // Arrange - Preparação
////            var whatsappNumero = "11999887766";
////            var canalId = 1;
////            var whatsappNormalizado = "5511999887766";
////            var canal = new Canal("Canal Teste", "Descrição", 1, 1, 1, 1, whatsappNormalizado, "config");
////            var leadStatusNovo = 1;
////            var responsavelPadrao = 10;

////            _redisCacheServiceMock.Setup(x => x.GetAsync<LeadRedisDTO>($"lead:whatsapp:{whatsappNormalizado}:canal:{canalId}"))
////                .ReturnsAsync((LeadRedisDTO)null);

////            _leadRepositoryMock.Setup(x => x.GetLeadByWhatsAppNumberAsync(whatsappNormalizado, canalId))
////                .ReturnsAsync((Domain.Entities.Lead.Lead)null);

////            _canalServiceMock.Setup(x => x.GetCanalByIdAsync(canalId))
////                .ReturnsAsync(canal);

////            _leadRepositoryMock.Setup(x => x.GetLeadStatusId("NOVO"))
////                .ReturnsAsync(leadStatusNovo);

////            _usuarioServiceMock.Setup(x => x.DistribuirReponsavel())
////                .ReturnsAsync(responsavelPadrao);

////            var novoLead = new Domain.Entities.Lead.Lead(whatsappNormalizado, leadStatusNovo, responsavelPadrao, canal.OrigemPadraoId, canal.EmpresaId);
////            novoLead.GetType().GetProperty("Id")?.SetValue(novoLead, 1);

////            _leadRepositoryMock.Setup(x => x.CreateAsync(It.IsAny<Domain.Entities.Lead.Lead>()))
////                .Callback<Domain.Entities.Lead.Lead>(lead => lead.GetType().GetProperty("Id")?.SetValue(lead, 1));

////            // Act - Ação
////            var resultado = await _leadService.VerificarLeadExistente(whatsappNumero, canalId);

//            // Assert - Verificação
//            Assert.NotNull(resultado);
//            Assert.Equal(1, resultado.LeadId);
//            Assert.Equal(responsavelPadrao, resultado.ResponsavelId);

////            // Verificações das chamadas
////            _canalServiceMock.Verify(x => x.GetCanalByIdAsync(canalId), Times.Once);
////            _leadRepositoryMock.Verify(x => x.GetLeadStatusId("NOVO"), Times.Once);
////            _usuarioServiceMock.Verify(x => x.DistribuirReponsavel(), Times.Once);
////            _leadRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Domain.Entities.Lead.Lead>()), Times.Once);
////            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
////            _redisCacheServiceMock.Verify(x => x.SetAsync(
////               $"lead:whatsapp:{whatsappNormalizado}:canal:{canalId}",
////               It.IsAny<LeadRedisDTO>(),
////               It.IsAny<TimeSpan?>()), Times.Once);

            
////        }

////        /// <summary>
////        /// Testa a validação de entrada quando o número WhatsApp é nulo ou vazio.
////        /// Verifica se uma AppException é lançada apropriadamente.
////        /// </summary>
////        [Fact]
////        public async Task VerificarLeadExistente_WhatsAppNumeroVazio_DeveLancarAppException()
////        {
////            // Arrange - Preparação
////            var canalId = 1;

////            // Act & Assert - Ação e Verificação para número vazio
////            var excecaoVazio = await Assert.ThrowsAsync<AppException>(() =>
////                _leadService.VerificarLeadExistente("", canalId));
////            Assert.Equal("Número do WhatsApp é obrigatório", excecaoVazio.Message);

////            // Act & Assert - Ação e Verificação para número nulo
////            var excecaoNulo = await Assert.ThrowsAsync<AppException>(() =>
////                _leadService.VerificarLeadExistente(null, canalId));
////            Assert.Equal("Número do WhatsApp é obrigatório", excecaoNulo.Message);

////            // Act & Assert - Ação e Verificação para número apenas com espaços
////            var excecaoEspacos = await Assert.ThrowsAsync<AppException>(() =>
////                _leadService.VerificarLeadExistente("   ", canalId));
////            Assert.Equal("Número do WhatsApp é obrigatório", excecaoEspacos.Message);
////        }

////        /// <summary>
////        /// Testa a validação de entrada quando o canal ID é inválido.
////        /// Verifica se uma AppException é lançada para valores inválidos.
////        /// </summary>
////        [Fact]
////        public async Task VerificarLeadExistente_CanalIdInvalido_DeveLancarAppException()
////        {
////            // Arrange - Preparação
////            var whatsappNumero = "11999887766";

////            // Act & Assert - Ação e Verificação para canal ID zero
////            var excecaoZero = await Assert.ThrowsAsync<AppException>(() =>
////                _leadService.VerificarLeadExistente(whatsappNumero, 0));
////            Assert.Equal("Canal ID deve ser maior que zero", excecaoZero.Message);

////            // Act & Assert - Ação e Verificação para canal ID negativo
////            var excecaoNegativo = await Assert.ThrowsAsync<AppException>(() =>
////                _leadService.VerificarLeadExistente(whatsappNumero, -1));
////            Assert.Equal("Canal ID deve ser maior que zero", excecaoNegativo.Message);
////        }

////        /// <summary>
////        /// Testa o cenário onde o canal não existe durante a criação de um novo lead.
////        /// Verifica se uma AppException é lançada quando o canal é inválido.
////        /// </summary>
////        [Fact]
////        public async Task VerificarLeadExistente_CanalNaoExiste_DeveLancarAppException()
////        {
////            // Arrange - Preparação
////            var whatsappNumero = "11999887766";
////            var canalId = 999;
////            var whatsappNormalizado = "5511999887766";

////            _redisCacheServiceMock.Setup(x => x.GetAsync<LeadRedisDTO>($"lead:whatsapp:{whatsappNormalizado}:canal:{canalId}"))
////                .ReturnsAsync((LeadRedisDTO)null);

////            _leadRepositoryMock.Setup(x => x.GetLeadByWhatsAppNumberAsync(whatsappNormalizado, canalId))
////                .ReturnsAsync((Domain.Entities.Lead.Lead)null);

////            _canalServiceMock.Setup(x => x.GetCanalByIdAsync(canalId))
////                .ReturnsAsync((Canal)null);

////            // Act & Assert - Ação e Verificação
////            var excecao = await Assert.ThrowsAsync<AppException>(() =>
////                _leadService.VerificarLeadExistente(whatsappNumero, canalId));
////            Assert.Equal("Canal não existe", excecao.Message);

////            _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Never);
////        }

////        /// <summary>
////        /// Testa o cenário onde o status "NOVO" não é encontrado no sistema.
////        /// Verifica se uma AppException é lançada quando a configuração do sistema está inconsistente.
////        /// </summary>
////        [Fact]
////        public async Task VerificarLeadExistente_StatusNovoNaoEncontrado_DeveLancarAppException()
////        {
////            // Arrange - Preparação
////            var whatsappNumero = "11999887766";
////            var canalId = 1;
////            var whatsappNormalizado = "5511999887766";
////            var canal = new Canal("Canal Teste", "Descrição", 1, 1, 1, 1, whatsappNormalizado, "config");

////            _redisCacheServiceMock.Setup(x => x.GetAsync<LeadRedisDTO>($"lead:whatsapp:{whatsappNormalizado}:canal:{canalId}"))
////                .ReturnsAsync((LeadRedisDTO)null);

////            _leadRepositoryMock.Setup(x => x.GetLeadByWhatsAppNumberAsync(whatsappNormalizado, canalId))
////                .ReturnsAsync((Domain.Entities.Lead.Lead)null);

////            _canalServiceMock.Setup(x => x.GetCanalByIdAsync(canalId))
////                .ReturnsAsync(canal);

////            _leadRepositoryMock.Setup(x => x.GetLeadStatusId("NOVO"))
////                .ReturnsAsync(0);

////            // Act & Assert - Ação e Verificação
////            var excecao = await Assert.ThrowsAsync<AppException>(() =>
////                _leadService.VerificarLeadExistente(whatsappNumero, canalId));
////            Assert.Equal("Status 'NOVO' não encontrado para leads", excecao.Message);

////            _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Never);
////        }

////        /// <summary>
////        /// Testa o cenário onde não há responsável padrão disponível.
////        /// Verifica se uma AppException é lançada quando não é possível distribuir um responsável.
////        /// </summary>
////        [Fact]
////        public async Task VerificarLeadExistente_ResponsavelPadraoNaoEncontrado_DeveLancarAppException()
////        {
////            // Arrange - Preparação
////            var whatsappNumero = "11999887766";
////            var canalId = 1;
////            var whatsappNormalizado = "5511999887766";
////            var canal = new Canal("Canal Teste", "Descrição", 1, 1, 1, 1, whatsappNormalizado, "config");
////            var leadStatusNovo = 1;

////            _redisCacheServiceMock.Setup(x => x.GetAsync<LeadRedisDTO>($"lead:whatsapp:{whatsappNormalizado}:canal:{canalId}"))
////                .ReturnsAsync((LeadRedisDTO)null);

////            _leadRepositoryMock.Setup(x => x.GetLeadByWhatsAppNumberAsync(whatsappNormalizado, canalId))
////                .ReturnsAsync((Domain.Entities.Lead.Lead)null);

////            _canalServiceMock.Setup(x => x.GetCanalByIdAsync(canalId))
////                .ReturnsAsync(canal);

////            _leadRepositoryMock.Setup(x => x.GetLeadStatusId("NOVO"))
////                .ReturnsAsync(leadStatusNovo);

////            _usuarioServiceMock.Setup(x => x.DistribuirReponsavel())
////                .ReturnsAsync(0);

////            // Act & Assert - Ação e Verificação
////            var excecao = await Assert.ThrowsAsync<AppException>(() =>
////                _leadService.VerificarLeadExistente(whatsappNumero, canalId));
////            Assert.Equal($"Responsável padrão não encontrado para canal {canalId}.", excecao.Message);

////            _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Never);
////        }

////        /// <summary>
////        /// Testa o cenário onde ocorre uma exceção inesperada durante o processo.
////        /// Verifica se o rollback é executado e uma AppException genérica é lançada.
////        /// </summary>
////        [Fact]
////        public async Task VerificarLeadExistente_ExcecaoInesperada_DeveRollbackELancarAppException()
////        {
////            // Arrange - Preparação
////            var whatsappNumero = "11999887766";
////            var canalId = 1;
////            var whatsappNormalizado = "5511999887766";

////            _redisCacheServiceMock.Setup(x => x.GetAsync<LeadRedisDTO>($"lead:whatsapp:{whatsappNormalizado}:canal:{canalId}"))
////                .ThrowsAsync(new Exception("Erro de conexão com Redis"));

////            // Act & Assert - Ação e Verificação
////            var excecao = await Assert.ThrowsAsync<AppException>(() =>
////                _leadService.VerificarLeadExistente(whatsappNumero, canalId));
////            Assert.Equal("Erro interno ao verificar lead existente", excecao.Message);
////            Assert.NotNull(excecao.InnerException);
////        }

////        /// <summary>
////        /// Testa a normalização de números WhatsApp com diferentes formatos.
////        /// Verifica se números com diferentes formatos são normalizados corretamente.
////        /// </summary>
////        [Theory]
////        [InlineData("11999887766", "5511999887766")] // Número com 11 dígitos
////        [InlineData("1199988776", "55111199988776")] // Número com 10 dígitos
////        [InlineData("(11) 99988-7766", "5511999887766")] // Número formatado
////        [InlineData("+55 11 99988-7766", "5511999887766")] // Número com código do país
////        [InlineData("55 11 99988-7766", "5511999887766")] // Número já com código do país
////        public async Task VerificarLeadExistente_NormalizacaoNumero_DeveNormalizarCorretamente(string numeroEntrada, string numeroEsperado)
////        {
////            // Arrange - Preparação
////            var canalId = 1;
////            var leadCache = new LeadRedisDTO(1, "Lead Teste", numeroEsperado, 10, 31);

////            _redisCacheServiceMock.Setup(x => x.GetAsync<LeadRedisDTO>($"lead:whatsapp:{numeroEsperado}:canal:{canalId}"))
////                .ReturnsAsync(leadCache);

////            // Act - Ação
////            var resultado = await _leadService.VerificarLeadExistente(numeroEntrada, canalId);

//            // Assert - Verificação
//            Assert.NotNull(resultado);
//            Assert.Equal(leadCache.Id, resultado.LeadId);
//            _redisCacheServiceMock.Verify(x => x.GetAsync<LeadRedisDTO>($"lead:whatsapp:{numeroEsperado}:canal:{canalId}"), Times.Once);
//        }
//    }
//}