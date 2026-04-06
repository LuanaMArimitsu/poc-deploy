using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Lead.Campanha;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Interfaces.Base;
using WebsupplyConnect.Domain.Interfaces.Lead;

namespace WebsupplyConnect.Application.Services.Lead
{
    public class CampanhaReaderService : ICampanhaReaderService
    {
        private readonly ILogger _logger;
        private readonly IUnitOfWork _unitOfWork;
        private readonly ICampanhaRepository _campanhaRepository;

        public CampanhaReaderService(
            ILogger<CampanhaReaderService> logger,
            IUnitOfWork unitOfWork,
            ICampanhaRepository campanhaRepository)
        {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
            _unitOfWork = unitOfWork ?? throw new ArgumentNullException(nameof(unitOfWork));
            _campanhaRepository = campanhaRepository ?? throw new ArgumentNullException(nameof(campanhaRepository));
        }

        public async Task<CampanhaPaginadaDTO> ListarCampanhasAsync(FiltroCampanhaDTO filtroCampanhaDTO)
        {
            try
            {
                var (itens, totalItens) = await _campanhaRepository.ListarCampanhasFiltroAsync(
                    filtroCampanhaDTO.Busca,
                    filtroCampanhaDTO.EmpresaId,
                    filtroCampanhaDTO.Codigo,
                    filtroCampanhaDTO.Ativa,
                    filtroCampanhaDTO.Temporaria,
                    filtroCampanhaDTO.EquipeId, 
                    filtroCampanhaDTO.DataCadastro,
                    filtroCampanhaDTO.DataInicio,
                    filtroCampanhaDTO.DataFim,
                    filtroCampanhaDTO.Pagina,
                    filtroCampanhaDTO.TamanhoPagina
                );

                var campanhasDto = itens.Select(c => new CampanhaDTO
                {
                    Id = c.Id,
                    Nome = c.Nome,
                    Codigo = c.Codigo,
                    Ativo = c.Ativo,
                    Temporaria = c.Temporaria,
                    DataInicio = c.DataInicio,
                    DataFim = c.DataFim,
                    EmpresaId = c.EmpresaId,
                    EquipeId = c.EquipeId ?? 0 
                }).ToList();

                int? totalPaginas = null;
                if (filtroCampanhaDTO.TamanhoPagina.HasValue && filtroCampanhaDTO.TamanhoPagina > 0)
                {
                    totalPaginas = (int)Math.Ceiling((double)totalItens / filtroCampanhaDTO.TamanhoPagina.Value);
                }

                return new CampanhaPaginadaDTO
                {
                    TotalItens = totalItens,
                    PaginaAtual = filtroCampanhaDTO.Pagina,
                    TotalPaginas = totalPaginas,
                    Itens = campanhasDto
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<Campanha?> CampanhaExistsByCodigoAsync(string codigo, int empresaId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(codigo))
                    throw new AppException("O código da campanha não pode ser nulo ou vazio.");

                var campanha = await _campanhaRepository.GetByPredicateAsync<WebsupplyConnect.Domain.Entities.Lead.Campanha>(
                    c => c.Codigo == codigo && c.EmpresaId == empresaId, false);
                return campanha;
            }
            catch (Exception ex)
            {
                throw new AppException(
                    $"Erro ao buscar a campanha pelo código '{codigo}' para a empresa ID {empresaId}. Detalhes: {ex.Message}"
                );
            }
        }

        public async Task<IEnumerable<ListCampanhaResponseDTO>> ListagemSimplesAsync(int empresaId)
        {
            try
            {
                var campanhas = await _campanhaRepository.ListagemSimplesAsync(empresaId);

                return campanhas.Select(c => new ListCampanhaResponseDTO
                {
                    Id = c.Id,
                    Nome = c.Nome
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar campanhas não transferidas para a empresa ID {empresaId}", empresaId);
                throw;
            }
        }

        public async Task<Campanha> CampanhaExistsByIdAsync(int campanhaId, int empresaId)
        {
            try
            {
                if (campanhaId <= 0)
                    throw new AppException("O ID da campanha deve ser maior que zero.");

                var campanha = await _campanhaRepository.GetByPredicateAsync<Campanha>(
                    c => c.Id == campanhaId && c.EmpresaId == empresaId && c.Excluido == false,
                    false
                );

                return campanha;
            }
            catch (Exception ex)
            {
                throw new AppException(
                    $"Erro ao buscar a campanha pelo ID '{campanhaId}' para a empresa ID {empresaId}. Detalhes: {ex.Message}"
                );
            }
        }

        public async Task<CampanhaDTO> ListarCampanhaByIdAsync(int campanhaId)
        {
            try
            {
                var campanha = await _campanhaRepository.GetByIdAsync<Campanha>(campanhaId, false);
                if (campanha == null)
                    throw new AppException($"Campanha com ID '{campanhaId}' não encontrada.");

                return new CampanhaDTO
                {
                    Id = campanha.Id,
                    Nome = campanha.Nome,
                    Codigo = campanha.Codigo,
                    Ativo = campanha.Ativo,
                    Temporaria = campanha.Temporaria,
                    DataInicio = campanha.DataInicio,
                    DataFim = campanha.DataFim,
                    EmpresaId = campanha.EmpresaId,
                    EquipeId = campanha.EquipeId.Value
                };
            }
            catch (Exception ex)
            {
                throw new AppException(
                    $"Erro ao buscar a campanha pelo ID '{campanhaId}'. Detalhes: {ex.Message}"
                );
            }
        }

        public async Task<List<Campanha>> ListarCampanhasNaoExcluidasParaETLAsync()
        {
            return await _campanhaRepository.GetListByPredicateAsync<Campanha>(c => !c.Excluido, includeDeleted: true);
        }
    }
}
