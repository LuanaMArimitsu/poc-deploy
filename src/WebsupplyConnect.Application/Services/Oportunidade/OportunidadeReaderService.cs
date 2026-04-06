using System.Data;
using Microsoft.Extensions.Logging;
using WebsupplyConnect.Application.Common;
using WebsupplyConnect.Application.DTOs.Oportunidade;
using WebsupplyConnect.Application.Interfaces.Oportunidade;
using WebsupplyConnect.Domain.Entities.Oportunidade;
using WebsupplyConnect.Domain.Interfaces.Oportunidade;

namespace WebsupplyConnect.Application.Services.Oportunidade
{
    public class OportunidadeReaderService(ILogger<OportunidadeReaderService> logger, IOportunidadeRepository oportunidadeRepository) : IOportunidadeReaderService
    {
        private readonly ILogger<OportunidadeReaderService> _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        private readonly IOportunidadeRepository _oportunidadeRepository = oportunidadeRepository ?? throw new ArgumentNullException(nameof(oportunidadeRepository));

        public async Task<Domain.Entities.Oportunidade.Oportunidade> GetOportunidadeByIdAsync(int id)
        {
            try
            {
                var oportunidade = await _oportunidadeRepository.GetByIdAsync<Domain.Entities.Oportunidade.Oportunidade>(id) ?? throw new AppException($"Oportunidade com ID igual a {id} não existe.");

                return oportunidade;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter uma oportunidade por ID.");
                throw;
            }
        }

        public async Task<GetOportunidadeDTO> GetOportunidadeByIdDetalhadoAsync(int id)
        {
            try
            {
                var oportunidade = await _oportunidadeRepository.GetDetailsById(id) ?? throw new AppException($"Oportunidade com ID igual a {id} não existe.");

                // Evento
                bool temEvento = oportunidade.LeadEventoId.HasValue && oportunidade.LeadEventoId != null;
                int? idEvento = temEvento ? oportunidade.LeadEventoId : null;
                string? campanhaDoEventoNome = temEvento && oportunidade.LeadEvento.CampanhaId.HasValue ? oportunidade.LeadEvento.Campanha.Nome : null;
                string? canalDoEvento = temEvento && oportunidade.LeadEvento.CanalId.HasValue ? oportunidade.LeadEvento.Canal.Nome : null;
                string? observacaoDoEvento = temEvento ? oportunidade.LeadEvento.Observacao : null;

                var oportunidadeDto = new GetOportunidadeDTO
                {
                    Id = oportunidade.Id,
                    EmpresaId = oportunidade.EmpresaId,
                    LeadId = oportunidade.LeadId,
                    ProdutoId = oportunidade.ProdutoId,
                    EtapaId = oportunidade.EtapaId,
                    ResponsavelId = oportunidade.ResponsavelId,
                    OrigemId = oportunidade.OrigemId,
                    NivelInteresse = oportunidade.Lead.NivelInteresse,
                    NomeEmpresa = oportunidade.Empresa.Nome,
                    NomeLead = oportunidade.Lead.Nome,
                    NomeProduto = oportunidade.Produto.Nome,
                    NomeEtapa = oportunidade.Etapa.Nome,
                    Valor = oportunidade.Valor,
                    NomeResponsavel = oportunidade.Responsavel.Nome,
                    NomeOrigem = oportunidade.Origem.Nome,
                    TipoInteresseId = oportunidade.TipoInteresseId,
                    NomeInteresse = oportunidade.TipoInteresse?.Titulo,
                    Probabilidade = oportunidade.Probabilidade,
                    Observacoes = oportunidade.Observacoes,
                    ValorFinal = oportunidade.ValorFinal,
                    CodEventoNBS = oportunidade.CodEvento,
                    ConvertidaNBS = oportunidade.Convertida.HasValue ? oportunidade.Convertida.Value : false,
                    DataPrevisaoFechamento = oportunidade.DataPrevisaoFechamento,
                    DataFechamento = oportunidade.DataFechamento,
                    DataUltimaInteracao = oportunidade.DataUltimaInteracao,
                    DataCriacao = oportunidade.DataCriacao,
                    // Campos de evento
                    TemEvento = temEvento,
                    IdEvento = idEvento,
                    CampanhaDoEventoNome = campanhaDoEventoNome,
                    CanalDoEvento = canalDoEvento,
                    ObservacaoDoEvento = observacaoDoEvento
                };

                return oportunidadeDto;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter uma oportunidade por ID.");
                throw;
            }
        }

        public async Task<OportunidadePaginadoDTO> GetOportunidadesAsync(FilterOportunidadeDTO filtro)
        {
            try
            {
                var (oportunidades, totalItens) = await _oportunidadeRepository.ListarOportunidadesFiltradoAsync(
                    filtro.LeadId,
                    filtro.ProdutoId,
                    filtro.EtapaId,
                    filtro.ValorMinimo,
                    filtro.ValorMaximo,
                    filtro.ResponsavelId,
                    filtro.OrigemId,
                    filtro.EmpresaId,
                    filtro.DataPrevisaoFechamento,
                    filtro.Pagina,
                    filtro.TamanhoPagina,
                    filtro.DataInicio,
                    filtro.DataFim);

                var totalPaginas = 0;
                if (filtro.Pagina > 0 && filtro.TamanhoPagina > 0)
                {

                    totalPaginas = (int)Math.Ceiling(totalItens / (double)filtro.TamanhoPagina);
                }

                var itens = oportunidades.Select(l =>
                {
                    bool temEvento = l.LeadEventoId.HasValue && l.LeadEventoId != null;
                    int? idEvento = temEvento ? l.LeadEventoId : null;
                    string? campanhaDoEventoNome = temEvento && l.LeadEvento.Campanha != null ? l.LeadEvento.Campanha.Nome : null;
                    string? canalDoEvento = temEvento && l.LeadEvento.CanalId.HasValue ? l.LeadEvento.Canal.Nome : null;
                    string? observacaoDoEvento = temEvento ? l.LeadEvento.Observacao : null;

                    return new GetOportunidadeDTO
                    {
                        Id = l.Id,
                        EmpresaId = l.EmpresaId,
                        LeadId = l.LeadId,
                        ProdutoId = l.ProdutoId,
                        EtapaId = l.EtapaId,
                        ResponsavelId = l.ResponsavelId,
                        OrigemId = l.OrigemId,
                        NivelInteresse = l.Lead.NivelInteresse,
                        NomeEmpresa = l.Empresa.Nome,
                        NomeLead = l.Lead.Nome,
                        NomeProduto = l.Produto.Nome,
                        NomeEtapa = l.Etapa.Nome,
                        Valor = l.Valor,
                        NomeResponsavel = l.Responsavel.Nome,
                        NomeOrigem = l.Origem.Nome,
                        Probabilidade = l.Probabilidade,
                        Observacoes = l.Observacoes,
                        ValorFinal = l.ValorFinal,
                        TipoInteresseId = l.TipoInteresseId,
                        CodEventoNBS = l.CodEvento,
                        ConvertidaNBS = l.Convertida.HasValue ? l.Convertida.Value : false,
                        NomeInteresse = l.TipoInteresse?.Titulo,
                        DataPrevisaoFechamento = l.DataPrevisaoFechamento,
                        DataFechamento = l.DataFechamento,
                        DataUltimaInteracao = l.DataUltimaInteracao,
                        DataCriacao = l.DataCriacao,
                        // Evento infos
                        TemEvento = temEvento,
                        IdEvento = idEvento,
                        CampanhaDoEventoNome = campanhaDoEventoNome,
                        CanalDoEvento = canalDoEvento,
                        ObservacaoDoEvento = observacaoDoEvento
                    };
                })
                .ToList();

                return new OportunidadePaginadoDTO
                {
                    TotalItens = totalItens,
                    PaginaAtual = filtro.Pagina,
                    TotalPaginas = totalPaginas,
                    Oportunidades = itens
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter todas as oportunidades.");
                throw;
            }
        }

        public async Task<List<Domain.Entities.Oportunidade.Oportunidade>> GetListOportunidadesByLeadIdAsync(int leadId)
        {
            try
            {
                if (leadId <= 0 || !await _oportunidadeRepository.ExistsInDatabaseAsync<Domain.Entities.Lead.Lead>(leadId))
                    throw new AppException($"Lead com ID igual a {leadId} não existe.");
                return await _oportunidadeRepository.GetListByPredicateAsync<Domain.Entities.Oportunidade.Oportunidade>(e => e.LeadId == leadId && !e.Excluido);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Erro ao obter oportunidades por ID do lead.");
                throw;
            }
        }

        public async Task<List<TipoInteresseDTO>> ListarTiposInteresseAsync()
        {
            var tipos = await _oportunidadeRepository.ListarTiposInteresseAsync();
            return tipos.Select(t => new TipoInteresseDTO { Id = t.Id, Titulo = t.Titulo }).ToList();
        }

        public async Task<List<Domain.Entities.Oportunidade.Oportunidade>> ObterOportunidadesPorPeriodoParaETLAsync(DateTime dataInicio, DateTime dataFim)
        {
            return await _oportunidadeRepository.GetListByPredicateAsync<Domain.Entities.Oportunidade.Oportunidade>(o =>
                (o.DataCriacao >= dataInicio && o.DataCriacao <= dataFim) ||
                (o.DataModificacao >= dataInicio && o.DataModificacao <= dataFim), includeDeleted: true);
        }

        public async Task<List<Domain.Entities.Oportunidade.Oportunidade>> ObterOportunidadesPorLeadIdParaETLAsync(int leadId)
        {
            return await _oportunidadeRepository.GetListByPredicateAsync<Domain.Entities.Oportunidade.Oportunidade>(o => o.LeadId == leadId, includeDeleted: true);
        }

        public async Task<List<Domain.Entities.Oportunidade.Oportunidade>> ObterOportunidadesPorLeadEventoIdParaETLAsync(int leadEventoId)
        {
            return await _oportunidadeRepository.GetListByPredicateAsync<Domain.Entities.Oportunidade.Oportunidade>(o => o.LeadEventoId == leadEventoId, includeDeleted: true);
        }

        public async Task<List<Domain.Entities.Oportunidade.Oportunidade>> ObterOportunidadesPorIdsParaETLAsync(IEnumerable<int> oportunidadeIds)
        {
            var ids = oportunidadeIds.Distinct().ToList();
            if (ids.Count == 0)
                return [];

            return await _oportunidadeRepository.GetListByPredicateAsync<Domain.Entities.Oportunidade.Oportunidade>(
                o => ids.Contains(o.Id),
                includeDeleted: true);
        }

        public async Task<string?> ObterNomeProdutoOportunidadeParaETLAsync(int oportunidadeId)
        {
            var oportunidade = await _oportunidadeRepository.GetDetailsById(oportunidadeId);
            return oportunidade?.Produto?.Nome;
        }

        public async Task<List<EtapaHistorico>> ObterHistoricoEtapasPorOportunidadeIdAsync(int oportunidadeId)
        {
            var historico = await _oportunidadeRepository.ObterHistoricoEtapasPorOportunidadeIdAsync(oportunidadeId);
            return historico;
        }
        public async Task<Domain.Entities.Oportunidade.Oportunidade?> GetPrimeiraOportunidadeAsync(int leadId)
        {
            var primeiraOportunidade = await _oportunidadeRepository.GetListByPredicateAsync<Domain.Entities.Oportunidade.Oportunidade>(
                o => o.LeadId == leadId,
                q => q.OrderBy(o => o.DataCriacao),
                includeDeleted: true);

            return primeiraOportunidade.FirstOrDefault();
        }

        public async Task<Domain.Entities.Oportunidade.Oportunidade?> ObterOportunidadePorIdParaETLAsync(int id)
        {
            return await _oportunidadeRepository.GetByIdAsync<Domain.Entities.Oportunidade.Oportunidade>(id, includeDeleted: true);
        }
    }
}
