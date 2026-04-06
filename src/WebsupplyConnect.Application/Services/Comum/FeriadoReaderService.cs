using FluentValidation;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Comum;
using WebsupplyConnect.Application.Interfaces.Comum;
using WebsupplyConnect.Domain.Entities.Comum;
using WebsupplyConnect.Domain.Interfaces.Comum;

namespace WebsupplyConnect.Application.Services.Comum
{
    /// <summary>
    /// Implementação do serviço de feriados sem dependência do AutoMapper
    /// </summary>
    public class FeriadoReaderService : IFeriadoReaderService
    {
        private readonly IFeriadoRepository _feriadoRepository;
        private readonly ILogger<FeriadoReaderService> _logger;

        /// <summary>
        /// Construtor do serviço
        /// </summary>
        public FeriadoReaderService(
            IFeriadoRepository feriadoRepository,
            IValidator<FeriadoCriarDTO> validatorCriar,
            IValidator<FeriadoAtualizarDTO> validatorAtualizar,
            ILogger<FeriadoReaderService> logger)
        {
            _feriadoRepository = feriadoRepository ?? throw new ArgumentNullException(nameof(feriadoRepository));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Obtém todos os feriados
        /// </summary>
        public async Task<List<FeriadoDTO>> ObterTodosAsync()
        {
            try
            {
                _logger.LogInformation("Obtendo todos os feriados");

                var feriados = await _feriadoRepository.GetAllAsync();
                
                // Mapear manualmente para lista de DTOs
                var feriadoDTOs = new List<FeriadoDTO>();
                foreach (var feriado in feriados)
                {
                    feriadoDTOs.Add(MapToDTO(feriado));
                }
                
                return feriadoDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter todos os feriados: {Message}", ex.Message);
                throw new AppException($"Erro ao obter todos os feriados: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtém um feriado pelo seu ID
        /// </summary>
        public async Task<FeriadoDTO> ObterPorIdAsync(int id)
        {
            try
            {
                _logger.LogInformation("Obtendo feriado por ID: {Id}", id);

                var feriado = await _feriadoRepository.GetByIdAsync<Feriado>(id);
                if (feriado == null)
                {
                    _logger.LogWarning("Feriado não encontrado. ID: {Id}", id);
                    throw new NotFoundAppException($"Feriado com ID {id} não encontrado");
                }

                return MapToDTO(feriado);
            }
            catch (NotFoundAppException)
            {
                // Já tratada, apenas propaga
                throw;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter feriado por ID: {Message}", ex.Message);
                throw new AppException($"Erro ao obter feriado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtém todos os feriados para uma empresa específica, incluindo feriados nacionais
        /// </summary>
        public async Task<List<FeriadoDTO>> ObterFeriadosPorEmpresaAsync(int empresaId, int? ano = null)
        {
            try
            {
                _logger.LogInformation("Obtendo feriados para empresa ID: {EmpresaId}, ano: {Ano}", 
                    empresaId, ano?.ToString() ?? "todos");

                var feriados = await _feriadoRepository.ObterFeriadosEmpresaAsync(empresaId, ano);
                
                // Mapear manualmente para lista de DTOs
                var feriadoDTOs = new List<FeriadoDTO>();
                foreach (var feriado in feriados)
                {
                    feriadoDTOs.Add(MapToDTO(feriado));
                }
                
                return feriadoDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter feriados por empresa: {Message}", ex.Message);
                throw new AppException($"Erro ao obter feriados por empresa: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Verifica se uma data específica é feriado para uma empresa
        /// </summary>
        public async Task<bool> VerificarDataFeriadoAsync(DateTime data, int? empresaId = null, bool considerarRecorrentes = true)
        {
            try
            {
                _logger.LogInformation("Verificando se {Data} é feriado {EmpresaInfo}", 
                    data.ToShortDateString(), 
                    empresaId.HasValue ? $"para empresa ID: {empresaId}" : "em geral");

                return await _feriadoRepository.VerificarDataFeriadoAsync(data, empresaId, considerarRecorrentes);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao verificar data de feriado: {Message}", ex.Message);
                throw new AppException($"Erro ao verificar data de feriado: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtém os próximos feriados a partir da data atual
        /// </summary>
        public async Task<List<FeriadoDTO>> ObterProximosFeriadosAsync(int empresaId, int quantidade = 5)
        {
            try
            {
                _logger.LogInformation("Obtendo {Quantidade} próximos feriados para empresa ID: {EmpresaId}", 
                    quantidade, empresaId);

                var feriados = await _feriadoRepository.ObterProximosFeriadosAsync(empresaId, quantidade);
                
                // Mapear manualmente para lista de DTOs
                var feriadoDTOs = new List<FeriadoDTO>();
                foreach (var feriado in feriados)
                {
                    feriadoDTOs.Add(MapToDTO(feriado));
                }
                
                return feriadoDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter próximos feriados: {Message}", ex.Message);
                throw new AppException($"Erro ao obter próximos feriados: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtém feriados por tipo
        /// </summary>
        public async Task<List<FeriadoDTO>> ObterFeriadosPorTipoAsync(string tipo, int? ano = null)
        {
            try
            {
                _logger.LogInformation("Obtendo feriados do tipo: {Tipo}, ano: {Ano}", 
                    tipo, ano?.ToString() ?? "todos");

                var feriados = await _feriadoRepository.ObterFeriadosPorTipoAsync(tipo, ano);
                
                // Mapear manualmente para lista de DTOs
                var feriadoDTOs = new List<FeriadoDTO>();
                foreach (var feriado in feriados)
                {
                    feriadoDTOs.Add(MapToDTO(feriado));
                }
                
                return feriadoDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter feriados por tipo: {Message}", ex.Message);
                throw new AppException($"Erro ao obter feriados por tipo: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Obtém feriados por UF
        /// </summary>
        public async Task<List<FeriadoDTO>> ObterFeriadosPorUFAsync(string uf, int? ano = null)
        {
            try
            {
                _logger.LogInformation("Obtendo feriados da UF: {UF}, ano: {Ano}", 
                    uf, ano?.ToString() ?? "todos");

                var feriados = await _feriadoRepository.ObterFeriadosPorUFAsync(uf, ano);
                
                // Mapear manualmente para lista de DTOs
                var feriadoDTOs = new List<FeriadoDTO>();
                foreach (var feriado in feriados)
                {
                    feriadoDTOs.Add(MapToDTO(feriado));
                }
                
                return feriadoDTOs;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter feriados por UF: {Message}", ex.Message);
                throw new AppException($"Erro ao obter feriados por UF: {ex.Message}", ex);
            }
        }
        /// <summary>
        /// Método auxiliar para mapear uma entidade Feriado para um DTO
        /// </summary>
        /// <param name="feriado">Entidade Feriado a ser mapeada</param>
        /// <returns>FeriadoDTO correspondente</returns>
        private static FeriadoDTO MapToDTO(Feriado feriado)
        {
            return new FeriadoDTO
            {
                Id = feriado.Id,
                Nome = feriado.Nome,
                Data = feriado.Data,
                Descricao = feriado.Descricao,
                Tipo = feriado.Tipo,
                EmpresaId = feriado.EmpresaId,
                Recorrente = feriado.Recorrente,
                UF = feriado.UF,
                CodigoMunicipio = feriado.CodigoMunicipio,
                DataCriacao = feriado.DataCriacao,
                DataModificacao = feriado.DataModificacao
            };
        }
    }
}