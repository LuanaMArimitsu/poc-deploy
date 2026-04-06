using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Lead.Evento;
using WebsupplyConnect.Application.DTOs.Lead.Historico;
using WebsupplyConnect.Application.Interfaces.Lead;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Interfaces.Lead;

namespace WebsupplyConnect.Application.Services.Lead
{
    public class LeadEventoReaderService : ILeadEventoReaderService
    {
        private readonly ILeadEventoRepository _repository;
        private readonly ILogger<LeadEventoReaderService> _logger;
        private readonly ILeadReaderService _leadReaderService;

        public LeadEventoReaderService(
            ILeadEventoRepository repository,
            ILogger<LeadEventoReaderService> logger,
            ILeadReaderService leadReader)
        {
            _repository = repository;
            _logger = logger;
            _leadReaderService = leadReader;
        }

        public async Task<List<LeadEventoResponseDTO>> GetAllAsync()
        {
            try
            {
                var historicos = await _repository.GetAllAsync();

                return historicos.Select(h => new LeadEventoResponseDTO
                {
                    Id = h.Id,
                    LeadId = h.LeadId,
                    LeadNome = h.Lead?.Nome,
                    OrigemId = h.OrigemId,
                    OrigemNome = h.Origem?.Nome,
                    CanalId = h.CanalId,
                    CanalNome = h.Canal?.Nome,
                    CampanhaId = h.CampanhaId,
                    CampanhaNome = h.Campanha?.Nome,
                    DataEvento = h.DataEvento,
                    Observacao = h.Observacao
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar todos os históricos de contato.");
                throw;
            }
        }

        public async Task<List<LeadEventoResponseDTO>> GetByLeadIdAsync(int leadId)
        {
            try
            {
                if (leadId <= 0)
                    throw new AppException("ID do Lead inválido.");

                var lead = await _leadReaderService.GetLeadByIdAsync(leadId);
                if (lead == null)
                    throw new AppException($"Lead com ID {leadId} não foi encontrado.");

                var historicos = await _repository.GetByLeadIdAsync(leadId);

                return historicos.Select(h => new LeadEventoResponseDTO
                {
                    Id = h.Id,
                    LeadId = h.LeadId,
                    LeadNome = h.Lead?.Nome,
                    OrigemId = h.OrigemId,
                    OrigemNome = h.Origem?.Nome,
                    CanalId = h.CanalId,
                    CanalNome = h.Canal?.Nome,
                    CampanhaId = h.CampanhaId,
                    CampanhaNome = h.Campanha?.Nome,
                    DataEvento = h.DataEvento,
                    Observacao = h.Observacao,
                    OportunidadesVinculadas = h.Oportunidades?.Select(o => o.Id.ToString()).ToArray() ?? Array.Empty<string>()
                }).ToList();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao listar históricos de contato para o lead {LeadId}.", leadId);
                throw;
            }
        }

        public async Task<EventosPaginadoDto> ListarEventosPorCampanhaAsync(ListEventoRequestDTO request)
        {
            if (request.CampanhaId <= 0)
                throw new AppException("CampanhaId deve ser maior que zero.");

            bool paginar = request.Pagina > 0 && request.TamanhoPagina > 0;

            int? pagina = paginar ? request.Pagina : null;
            int? tamanho = paginar ? request.TamanhoPagina : null;

            var (eventos, totalItens) = await _repository.ListEventosPorCampanhaAsync(
                campanhaId: request.CampanhaId,
                pagina: pagina,
                tamanhoPagina: tamanho
            );

            var itens = eventos
                .GroupBy(e => new { e.LeadId, e.Lead.Nome })
                .Select(g => new ListEventosPorLeadDTO
                {
                    LeadId = g.Key.LeadId,
                    Lead = g.Key.Nome,
                    Eventos = g
                        .Select(ev => new ListEventoCampanhaSimplesDTO
                        {
                            DataEvento = ev.DataEvento,
                            Origem = ev.Origem.Nome
                        })
                        .OrderBy(ev => ev.DataEvento)
                        .ToList()
                })
                .OrderBy(x => x.Lead)
                .ToList();

            var totalPaginas = paginar
                ? (int)Math.Ceiling(totalItens / (double)tamanho!.Value)
                : 1;

            return new EventosPaginadoDto
            {
                TotalItens = totalItens,
                PaginaAtual = paginar ? request.Pagina!.Value : 1,
                TotalPaginas = totalPaginas,
                Itens = itens
            };
        }

        public async Task<LeadEvento?> GetLeadEventoByIdAsync(int id)
        {
            try
            {
                if (id <= 0)
                    throw new AppException("ID do Evento inválido.");

                var evento = await _repository.GetLeadEventoByIdAsync(id);
                return evento;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao buscar LeadEvento por id {EventoId}.", id);
                throw;
            }
        }

        public async Task<List<LeadEvento>> ObterEventosPorPeriodoParaETLAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await _repository.GetListByPredicateAsync<LeadEvento>(e =>
                (e.DataEvento >= dataInicio && e.DataEvento <= dataFim) ||
                (e.DataModificacao >= dataInicio && e.DataModificacao <= dataFim), includeDeleted: true);
        }

        public async Task<List<LeadEvento>> ObterEventosPorLeadIdParaETLAsync(int leadId)
        {
            return await _repository.GetListByPredicateAsync<LeadEvento>(e => e.LeadId == leadId, includeDeleted: true);
        }

        public async Task<Dictionary<int, List<LeadEvento>>> ObterEventosAgrupadosPorLeadIdsParaETLAsync(IEnumerable<int> leadIds)
        {
            var ids = leadIds.Distinct().ToList();
            if (ids.Count == 0)
                return new Dictionary<int, List<LeadEvento>>();

            var todos = await _repository.GetListByPredicateAsync<LeadEvento>(e => ids.Contains(e.LeadId), includeDeleted: true);
            return todos
                .GroupBy(e => e.LeadId)
                .ToDictionary(g => g.Key, g => g.ToList());
        }
    }
}
