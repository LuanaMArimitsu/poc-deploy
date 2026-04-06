//using FluentValidation;
//using FluentValidation.Results;
//using Microsoft.Extensions.Logging;
//using Moq;
//using WebsupplyConnect.Application.Common;
//using WebsupplyConnect.Application.DTOs.Comunicacao;
//using WebsupplyConnect.Application.DTOs.Redis;
//using WebsupplyConnect.Application.Interfaces.ExternalServices;
//using WebsupplyConnect.Application.Services.Comunicacao;
//using WebsupplyConnect.Domain.Entities.Comunicacao;
//using WebsupplyConnect.Domain.Entities.Empresa;
//using WebsupplyConnect.Domain.Interfaces.Base;
//using WebsupplyConnect.Domain.Interfaces.Comunicacao;

//namespace WebsupplyConnect.Application.Tests.Services
//{
//    public class CanalServiceTests
//    {
//        private readonly Mock<IUnitOfWork> _unitOfWorkMock;
//        private readonly Mock<ICanalRepository> _canalRepositoryMock;
//        private readonly Mock<ILogger<CanalReaderService>> _loggerMock;
//        private readonly Mock<IValidator<CreateCanalDTO>> _validatorMock;
//        private readonly Mock<IRedisCacheService> _redisCacheServiceMock;
//        private readonly Mock<CanalReaderService> _mockCanalService;

//        public CanalServiceTests()
//        {
//            _unitOfWorkMock = new Mock<IUnitOfWork>();
//            _canalRepositoryMock = new Mock<ICanalRepository>();
//            _loggerMock = new Mock<ILogger<CanalReaderService>>();
//            _validatorMock = new Mock<IValidator<CreateCanalDTO>>();
//            _redisCacheServiceMock = new Mock<IRedisCacheService>();

//            _mockCanalService = new Mock<CanalReaderService>(
//                _unitOfWorkMock.Object,
//                _canalRepositoryMock.Object,
//                _loggerMock.Object,
//                _validatorMock.Object,
//                _redisCacheServiceMock.Object
//            )
//            { CallBase = true };
//        }

//        /// <summary>
//        /// Testes com todas as validações, deve dar sucesso
//        /// Valida a lógica de criação de um canal com DTO válido
//        /// </summary>
//        [Fact]
//        public async Task Create_DtoValido_DeveCriarCanalComSucesso()
//        {
//            // Arrange - Preparação
//            var createDto = new CreateCanalDTO(
//                "Canal Teste",
//                "Descrição do canal teste",
//                1,
//                1,
//                1,
//                100,
//                "5511999887766",
//                "{}"
//            );

//            var resultadoValidacao = new ValidationResult();
//            _validatorMock.Setup(x => x.ValidateAsync(createDto, default))
//                .ReturnsAsync(resultadoValidacao);

//            _canalRepositoryMock.Setup(x => x.ExistsInDatabaseAsync<Empresa>(createDto.EmpresaId))
//                .ReturnsAsync(true);

//            var canalTipo = new CanalTipo(1, "WHATSAPP", "WhatsApp", "Canal WhatsApp", 1, new DateTime(2025, 1, 1), new DateTime(2025, 1, 1));
//            _canalRepositoryMock.Setup(x => x.GetByIdAsync<CanalTipo>(createDto.CanalTipoId, false))
//                .ReturnsAsync(canalTipo);

//            _canalRepositoryMock.Setup(x => x.GetCanalByWhatsAppNumberAsync(createDto.WhatsAppNumero))
//                .ReturnsAsync((Canal)null);

//            _canalRepositoryMock.Setup(x => x.CanalNameExistsAsync(createDto.Nome))
//                .ReturnsAsync(false);

//            // Act - Ação
//            await _mockCanalService.Object.Create(createDto);

//            // Assert - Verificação das regras de validação (ValidateRulesAsync)
//            _canalRepositoryMock.Verify(x => x.ExistsInDatabaseAsync<Empresa>(createDto.EmpresaId), Times.Once);
//            _canalRepositoryMock.Verify(x => x.GetByIdAsync<CanalTipo>(createDto.CanalTipoId, false), Times.Once);
//            _canalRepositoryMock.Verify(x => x.GetCanalByWhatsAppNumberAsync(createDto.WhatsAppNumero), Times.Once);
//            _canalRepositoryMock.Verify(x => x.CanalNameExistsAsync(createDto.Nome), Times.Once);

//            // Assert - Verificação do fluxo principal
//            _canalRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Canal>()), Times.Once);
//            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Once);
//        }

//        /// <summary>
//        /// Testes com todas as validações, deve dar erro
//        /// Valida a lógica de criação de um canal com DTO invalido
//        /// </summary>
//        [Fact]
//        public async Task Create_DtoInvalido_DeveLancarAppException()
//        {
//            // Arrange - Preparação
//            var createDto = new CreateCanalDTO(
//                "", // Nome inválido
//                "Descrição",
//                1,
//                1,
//                1,
//                null,
//                null,
//                null
//            );

//            var falhaValidacao = new ValidationFailure("Nome", "Nome é obrigatório");
//            var resultadoValidacao = new ValidationResult(new[] { falhaValidacao });

//            _validatorMock.Setup(x => x.ValidateAsync(createDto, default))
//                .ReturnsAsync(resultadoValidacao);

//            // Act & Assert - Ação e Verificação
//            var excecao = await Assert.ThrowsAsync<AppException>(() => _mockCanalService.Object.Create(createDto));
//            Assert.Contains("Dados inválidos para criação do canal", excecao.Message);

//            // Assert - Verificação que ValidateRulesAsync NÃO foi chamado
//            _canalRepositoryMock.Verify(x => x.ExistsInDatabaseAsync<Empresa>(It.IsAny<int>()), Times.Never);
//            _canalRepositoryMock.Verify(x => x.GetByIdAsync<CanalTipo>(It.IsAny<int>(), It.IsAny<bool>()), Times.Never);
//            _canalRepositoryMock.Verify(x => x.GetCanalByWhatsAppNumberAsync(It.IsAny<string>()), Times.Never);
//            _canalRepositoryMock.Verify(x => x.CanalNameExistsAsync(It.IsAny<string>()), Times.Never);

//            _canalRepositoryMock.Verify(x => x.CreateAsync(It.IsAny<Canal>()), Times.Never);
//            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
//        }

//        /// <summary>
//        /// Testa o cenário onde a empresa especificada no DTO não existe no banco de dados.
//        /// Verifica se a primeira validação de regra de negócio falha apropriadamente.
//        /// </summary>
//        [Fact]
//        public async Task Create_EmpresaNaoExiste_DeveLancarAppException()
//        {
//            // Arrange - Preparação
//            var createDto = new CreateCanalDTO(
//                "Canal Teste",
//                "Descrição",
//                1,
//                999, // Empresa inexistente
//                1,
//                null,
//                null,
//                null
//            );

//            var resultadoValidacao = new ValidationResult();
//            _validatorMock.Setup(x => x.ValidateAsync(createDto, default))
//                .ReturnsAsync(resultadoValidacao);

//            _canalRepositoryMock.Setup(x => x.ExistsInDatabaseAsync<Empresa>(createDto.EmpresaId))
//                .ReturnsAsync(false);

//            // Act & Assert - Ação e Verificação
//            var excecao = await Assert.ThrowsAsync<AppException>(() => _mockCanalService.Object.Create(createDto));
//            Assert.Equal($"Empresa com ID {createDto.EmpresaId} não encontrada", excecao.Message);

//            // Assert - Verificação que ValidateRulesAsync foi chamado e falhou na primeira validação
//            _canalRepositoryMock.Verify(x => x.ExistsInDatabaseAsync<Empresa>(createDto.EmpresaId), Times.Once);

//            // Não deve fazer rollback pois nenhuma transação foi iniciada (apenas validações)
//            _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Never);
//            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
//        }

//        /// <summary>
//        /// Testa o cenário onde o tipo de canal especificado não existe no banco de dados.
//        /// Verifica se a validação falha após validar a empresa mas antes das demais validações.
//        /// </summary>
//        [Fact]
//        public async Task Create_TipoCanalNaoExiste_DeveLancarAppException()
//        {
//            // Arrange - Preparação
//            var createDto = new CreateCanalDTO(
//                "Canal Teste",
//                "Descrição",
//                999, // Tipo inexistente
//                1,
//                1,
//                null,
//                null,
//                null
//            );

//            var resultadoValidacao = new ValidationResult();
//            _validatorMock.Setup(x => x.ValidateAsync(createDto, default))
//                .ReturnsAsync(resultadoValidacao);

//            _canalRepositoryMock.Setup(x => x.ExistsInDatabaseAsync<Empresa>(createDto.EmpresaId))
//                .ReturnsAsync(true);

//            _canalRepositoryMock.Setup(x => x.GetByIdAsync<CanalTipo>(createDto.CanalTipoId, false))
//                .ReturnsAsync((CanalTipo)null);

//            // Act & Assert - Ação e Verificação
//            var excecao = await Assert.ThrowsAsync<AppException>(() => _mockCanalService.Object.Create(createDto));
//            Assert.Equal($"Tipo de canal com ID {createDto.CanalTipoId} não encontrado", excecao.Message);

//            // Assert - Verificação que ValidateRulesAsync foi chamado até a segunda validação
//            _canalRepositoryMock.Verify(x => x.ExistsInDatabaseAsync<Empresa>(createDto.EmpresaId), Times.Once);
//            _canalRepositoryMock.Verify(x => x.GetByIdAsync<CanalTipo>(createDto.CanalTipoId, false), Times.Once);

//            // Não deve fazer rollback pois nenhuma transação foi iniciada (apenas validações)
//            _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Never);
//            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
//        }

//        /// <summary>
//        /// Testa o cenário onde um canal do tipo WhatsApp é criado sem fornecer o número obrigatório.
//        /// Verifica se a validação específica para canais WhatsApp funciona corretamente.
//        /// </summary>
//        [Fact]
//        public async Task Create_CanalWhatsAppSemNumero_DeveLancarAppException()
//        {
//            // Arrange - Preparação
//            var createDto = new CreateCanalDTO(
//                "Canal WhatsApp",
//                "Descrição",
//                1,
//                1,
//                1,
//                null,
//                "", // Número vazio para canal WhatsApp
//                null
//            );

//            var resultadoValidacao = new ValidationResult();
//            _validatorMock.Setup(x => x.ValidateAsync(createDto, default))
//                .ReturnsAsync(resultadoValidacao);

//            _canalRepositoryMock.Setup(x => x.ExistsInDatabaseAsync<Empresa>(createDto.EmpresaId))
//                .ReturnsAsync(true);

//            var canalTipo = new CanalTipo(1, "WHATSAPP", "WhatsApp", "Canal WhatsApp", 1, new DateTime(2025, 1, 1), new DateTime(2025, 1, 1));
//            _canalRepositoryMock.Setup(x => x.GetByIdAsync<CanalTipo>(createDto.CanalTipoId, false))
//                .ReturnsAsync(canalTipo);

//            // Act & Assert - Ação e Verificação
//            var excecao = await Assert.ThrowsAsync<AppException>(() => _mockCanalService.Object.Create(createDto));
//            Assert.Equal("O número do WhatsApp é obrigatório para o canal do tipo WHATSAPP", excecao.Message);

//            // Assert - Verificação que ValidateRulesAsync foi chamado até a validação do tipo
//            _canalRepositoryMock.Verify(x => x.ExistsInDatabaseAsync<Empresa>(createDto.EmpresaId), Times.Once);
//            _canalRepositoryMock.Verify(x => x.GetByIdAsync<CanalTipo>(createDto.CanalTipoId, false), Times.Once);

//            // Não deve fazer rollback pois nenhuma transação foi iniciada (apenas validações)
//            _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Never);
//            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
//        }

//        /// <summary>
//        /// Testa o cenário onde se tenta criar um canal com um número WhatsApp já em uso.
//        /// Verifica se a validação de unicidade do número WhatsApp funciona corretamente.
//        /// </summary>
//        [Fact]
//        public async Task Create_NumeroWhatsAppDuplicado_DeveLancarAppException()
//        {
//            // Arrange - Preparação
//            var createDto = new CreateCanalDTO(
//                "Canal Teste",
//                "Descrição",
//                1,
//                1,
//                1,
//                null,
//                "5511999887766",
//                null
//            );

//            var resultadoValidacao = new ValidationResult();
//            _validatorMock.Setup(x => x.ValidateAsync(createDto, default))
//                .ReturnsAsync(resultadoValidacao);

//            _canalRepositoryMock.Setup(x => x.ExistsInDatabaseAsync<Empresa>(createDto.EmpresaId))
//                .ReturnsAsync(true);

//            var canalTipo = new CanalTipo(1, "WHATSAPP", "WhatsApp", "Canal WhatsApp", 1, new DateTime(2025, 1, 1), new DateTime(2025, 1, 1));
//            _canalRepositoryMock.Setup(x => x.GetByIdAsync<CanalTipo>(createDto.CanalTipoId, false))
//                .ReturnsAsync(canalTipo);

//            var canalExistente = new Canal("Canal Existente", "Descrição", 1, 1, 1, 100, "5511999887766", "config");
//            _canalRepositoryMock.Setup(x => x.GetCanalByWhatsAppNumberAsync(createDto.WhatsAppNumero))
//                .ReturnsAsync(canalExistente);

//            // Act & Assert - Ação e Verificação
//            var excecao = await Assert.ThrowsAsync<AppException>(() => _mockCanalService.Object.Create(createDto));
//            Assert.Equal($"Já existe um canal cadastrado com o número WhatsApp: {createDto.WhatsAppNumero}", excecao.Message);

//            // Assert - Verificação que ValidateRulesAsync foi chamado até a validação do WhatsApp
//            _canalRepositoryMock.Verify(x => x.ExistsInDatabaseAsync<Empresa>(createDto.EmpresaId), Times.Once);
//            _canalRepositoryMock.Verify(x => x.GetByIdAsync<CanalTipo>(createDto.CanalTipoId, false), Times.Once);
//            _canalRepositoryMock.Verify(x => x.GetCanalByWhatsAppNumberAsync(createDto.WhatsAppNumero), Times.Once);

//            // Não deve fazer rollback pois nenhuma transação foi iniciada (apenas validações)
//            _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Never);
//            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
//        }

//        /// <summary>
//        /// Testa o cenário onde se tenta criar um canal com um nome já existente na empresa.
//        /// Verifica se a validação de unicidade do nome do canal funciona corretamente.
//        /// </summary>
//        [Fact]
//        public async Task Create_NomeCanalDuplicado_DeveLancarAppException()
//        {
//            // Arrange - Preparação
//            var createDto = new CreateCanalDTO(
//                "Canal Existente",
//                "Descrição",
//                1,
//                1,
//                1,
//                null,
//                "5511999887766",
//                null
//            );

//            var resultadoValidacao = new ValidationResult();
//            _validatorMock.Setup(x => x.ValidateAsync(createDto, default))
//                .ReturnsAsync(resultadoValidacao);

//            _canalRepositoryMock.Setup(x => x.ExistsInDatabaseAsync<Empresa>(createDto.EmpresaId))
//                .ReturnsAsync(true);

//            var canalTipo = new CanalTipo(1, "WHATSAPP", "WhatsApp", "Canal WhatsApp", 1, new DateTime(2025, 1, 1), new DateTime(2025, 1, 1));
//            _canalRepositoryMock.Setup(x => x.GetByIdAsync<CanalTipo>(createDto.CanalTipoId, false))
//                .ReturnsAsync(canalTipo);

//            _canalRepositoryMock.Setup(x => x.GetCanalByWhatsAppNumberAsync(createDto.WhatsAppNumero))
//                .ReturnsAsync((Canal)null);

//            _canalRepositoryMock.Setup(x => x.CanalNameExistsAsync(createDto.Nome))
//                .ReturnsAsync(true);

//            // Act & Assert - Ação e Verificação
//            var excecao = await Assert.ThrowsAsync<AppException>(() => _mockCanalService.Object.Create(createDto));
//            Assert.Equal($"Já existe um canal com o nome '{createDto.Nome}' nesta empresa", excecao.Message);

//            // Assert - Verificação que todas as validações de ValidateRulesAsync foram chamadas
//            _canalRepositoryMock.Verify(x => x.ExistsInDatabaseAsync<Empresa>(createDto.EmpresaId), Times.Once);
//            _canalRepositoryMock.Verify(x => x.GetByIdAsync<CanalTipo>(createDto.CanalTipoId, false), Times.Once);
//            _canalRepositoryMock.Verify(x => x.GetCanalByWhatsAppNumberAsync(createDto.WhatsAppNumero), Times.Once);
//            _canalRepositoryMock.Verify(x => x.CanalNameExistsAsync(createDto.Nome), Times.Once);

//            // Não deve fazer rollback pois nenhuma transação foi iniciada (apenas validações)
//            _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Never);
//            _unitOfWorkMock.Verify(x => x.CommitAsync(), Times.Never);
//        }

//        /// <summary>
//        /// Testa o cenário onde ocorre uma exceção inesperada durante o processo de criação.
//        /// Verifica se o rollback é executado e uma AppException genérica é lançada.
//        /// </summary>
//        [Fact]
//        public async Task Create_ExcecaoInesperada_DeveRollbackELancarAppException()
//        {
//            // Arrange - Preparação
//            var createDto = new CreateCanalDTO(
//                "Canal Teste",
//                "Descrição",
//                1,
//                1,
//                1,
//                null,
//                null,
//                null
//            );

//            var resultadoValidacao = new ValidationResult();
//            _validatorMock.Setup(x => x.ValidateAsync(createDto, default))
//                .ReturnsAsync(resultadoValidacao);

//            _canalRepositoryMock.Setup(x => x.ExistsInDatabaseAsync<Empresa>(createDto.EmpresaId))
//                .ThrowsAsync(new Exception("Erro de conexão com banco de dados"));

//            // Act & Assert - Ação e Verificação
//            var excecao = await Assert.ThrowsAsync<AppException>(() => _mockCanalService.Object.Create(createDto));
//            Assert.Equal("Error registering canal", excecao.Message);

//            // Assert - Verificação que tentou executar ValidateRulesAsync mas falhou
//            _canalRepositoryMock.Verify(x => x.ExistsInDatabaseAsync<Empresa>(createDto.EmpresaId), Times.Once);
//            _unitOfWorkMock.Verify(x => x.RollbackAsync(), Times.Never);
//            _unitOfWorkMock.Verify(x => x.SaveChangesAsync(), Times.Never);
//        }



//        /// <summary>
//        /// Testa se o método List retorna corretamente uma lista de canais do repositório.
//        /// Verifica se a chamada ao repositório é feita e os dados são retornados adequadamente.
//        /// </summary>
//        [Fact]
//        public async Task List_DeveRetornarListaDeCanais()
//        {
//            // Arrange - Preparação
//            var canaisEsperados = new List<Canal>
//            {
//                new Canal("Canal 1", "Descrição 1", 1, 1, 1, 100, "5511999887766", "config1"),
//                new Canal("Canal 2", "Descrição 2", 1, 1, 1, 200, "5511999887767", "config2")
//            };

//            _canalRepositoryMock.Setup(x => x.ListCanaisAsync(null, null))
//                .ReturnsAsync(canaisEsperados);

//            // Act - Ação
//            var resultado = await _mockCanalService.Object.List();

//            // Assert - Verificação
//            Assert.NotNull(resultado);
//            Assert.Equal(2, resultado.Count);
//            Assert.Equal(canaisEsperados, resultado);
//            _canalRepositoryMock.Verify(x => x.ListCanaisAsync(null, null), Times.Once);
//        }


//        /// <summary>
//        /// Testa se o método List retorna uma lista vazia quando não há canais cadastrados.
//        /// Verifica se o comportamento é adequado mesmo com resultado vazio do repositório.
//        /// </summary>
//        [Fact]
//        public async Task List_ResultadoVazio_DeveRetornarListaVazia()
//        {
//            // Arrange - Preparação
//            var canaisEsperados = new List<Canal>();

//            _canalRepositoryMock.Setup(x => x.ListCanaisAsync(null, null))
//                .ReturnsAsync(canaisEsperados);

//            // Act - Ação
//            var resultado = await _mockCanalService.Object.List();

//            // Assert - Verificação
//            Assert.NotNull(resultado);
//            Assert.Empty(resultado);
//            _canalRepositoryMock.Verify(x => x.ListCanaisAsync(null, null), Times.Once);
//        }

//        /// <summary>
//        /// Testa se uma exceção inesperada durante a listagem é tratada adequadamente.
//        /// Verifica se uma AppException é lançada quando ocorre erro no repositório.
//        /// </summary>
//        [Fact]
//        public async Task List_ExcecaoInesperada_DeveLancarAppException()
//        {
//            // Arrange - Preparação
//            _canalRepositoryMock.Setup(x => x.ListCanaisAsync(null, null))
//                .ThrowsAsync(new Exception("Erro no banco de dados"));

//            // Act & Assert - Ação e Verificação
//            var excecao = await Assert.ThrowsAsync<AppException>(() => _mockCanalService.Object.List());
//            Assert.Equal("Error listing canais", excecao.Message);
//        }

//        /// <summary>
//        /// Testa se o canal é encontrado e retornado corretamente quando existe no cache Redis.
//        /// Verifica se a otimização de cache funciona evitando consultas desnecessárias ao banco.
//        /// </summary>
//        [Fact]
//        public async Task GetCanalByWhatsAppAsync_CanalNoCache_DeveRetornarDoCache()
//        {
//            // Arrange - Preparação
//            var numeroWhatsapp = "5511999887766";
//            var canalCache = new CanalRedisDTO(1, "Canal Teste", 1, numeroWhatsapp, "{}");


//            _redisCacheServiceMock.Setup(x => x.GetAsync<CanalRedisDTO>($"canal:whatsapp:{numeroWhatsapp}"))
//                .ReturnsAsync(canalCache);

//            // Act - Ação
//            var resultado = await _mockCanalService.Object.GetCanalByWhatsAppAsync(numeroWhatsapp);

//            // Assert - Verificação
//            Assert.NotNull(resultado);
//            Assert.Equal(canalCache.Id, resultado.CanalId);
//            Assert.Equal(canalCache.EmpresaId, resultado.EmpresaId);

//            _redisCacheServiceMock.Verify(x => x.GetAsync<CanalRedisDTO>($"canal:whatsapp:{numeroWhatsapp}"), Times.Once);
//            _canalRepositoryMock.Verify(x => x.GetCanalByWhatsAppNumberAsync(It.IsAny<string>()), Times.Never);
//        }

//        /// <summary>
//        /// Testa o cenário onde o canal não está no cache mas existe no banco de dados.
//        /// Verifica se o canal é buscado no banco e posteriormente armazenado no cache.
//        /// </summary>
//        [Fact]
//        public async Task GetCanalByWhatsAppAsync_CanalNaoNoCacheMasNoBanco_DeveRetornarDoBancoEArmazenarNoCache()
//        {
//            // Arrange - Preparação
//            var numeroWhatsapp = "5511999887766";
//            var canalBanco = new Canal("Canal Teste", "Descrição", 1, 1, 1, 100, numeroWhatsapp, "config");

//            _redisCacheServiceMock.Setup(x => x.GetAsync<CanalRedisDTO>($"canal:whatsapp:{numeroWhatsapp}"))
//                .ReturnsAsync((CanalRedisDTO)null);

//            _canalRepositoryMock.Setup(x => x.GetCanalByWhatsAppNumberAsync(numeroWhatsapp))
//                .ReturnsAsync(canalBanco);

//            // Act - Ação
//            var resultado = await _mockCanalService.Object.GetCanalByWhatsAppAsync(numeroWhatsapp);

//            // Assert - Verificação
//            Assert.NotNull(resultado);
//            Assert.Equal(canalBanco.Id, resultado.CanalId);
//            Assert.Equal(canalBanco.EmpresaId, resultado.EmpresaId);

//            _redisCacheServiceMock.Verify(x => x.GetAsync<CanalRedisDTO>($"canal:whatsapp:{numeroWhatsapp}"), Times.Once);
//            _canalRepositoryMock.Verify(x => x.GetCanalByWhatsAppNumberAsync(numeroWhatsapp), Times.Once);
//            var chaveEsperada = $"canal:whatsapp:{numeroWhatsapp}";
//            _redisCacheServiceMock.Verify(x => x.SetAsync(
//                chaveEsperada,
//                It.IsAny<CanalRedisDTO>(),
//                It.IsAny<TimeSpan?>()), Times.Once);
//        }

//        /// <summary>
//        /// Testa o cenário onde o canal não existe nem no cache nem no banco de dados.
//        /// Verifica se null é retornado adequadamente quando o canal não é encontrado.
//        /// </summary>
//        [Fact]
//        public async Task GetCanalByWhatsAppAsync_CanalNaoEncontrado_DeveRetornarNull()
//        {
//            // Arrange - Preparação
//            var numeroWhatsapp = "5511999887766";

//            _redisCacheServiceMock.Setup(x => x.GetAsync<CanalRedisDTO>($"canal:whatsapp:{numeroWhatsapp}"))
//                .ReturnsAsync((CanalRedisDTO)null);

//            _canalRepositoryMock.Setup(x => x.GetCanalByWhatsAppNumberAsync(numeroWhatsapp))
//                .ReturnsAsync((Canal)null);

//            // Act - Ação
//            var resultado = await _mockCanalService.Object.GetCanalByWhatsAppAsync(numeroWhatsapp);

//            // Assert - Verificação
//            Assert.Null(resultado);

//            _redisCacheServiceMock.Verify(x => x.GetAsync<CanalRedisDTO>($"canal:whatsapp:{numeroWhatsapp}"), Times.Once);
//            _canalRepositoryMock.Verify(x => x.GetCanalByWhatsAppNumberAsync(numeroWhatsapp), Times.Once);
//            _redisCacheServiceMock.Verify(x => x.SetAsync(It.IsAny<string>(), It.IsAny<CanalRedisDTO>(), It.IsAny<TimeSpan?>()), Times.Never);
//        }

//        /// <summary>
//        /// Testa se o método trata adequadamente números WhatsApp nulos ou vazios.
//        /// Verifica se a validação de entrada funciona para parâmetros inválidos.
//        /// </summary>
//        [Fact]
//        public async Task GetCanalByWhatsAppAsync_NumeroWhatsAppNuloOuVazio()
//        {
//            // Arrange, Act & Assert - Preparação, Ação e Verificação para null
//            var resultadoNull = await _mockCanalService.Object.GetCanalByWhatsAppAsync(null);
//            Assert.Null(resultadoNull);

//            // Arrange, Act & Assert - Preparação, Ação e Verificação para vazio
//            var resultadoVazio = await _mockCanalService.Object.GetCanalByWhatsAppAsync("");
//            Assert.Null(resultadoVazio);
//        }
//    }
//}
