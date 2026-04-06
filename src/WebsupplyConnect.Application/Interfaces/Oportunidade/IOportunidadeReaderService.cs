using WebsupplyConnect.Application.DTOs.Oportunidade;
using WebsupplyConnect.Domain.Entities.Oportunidade;

namespace WebsupplyConnect.Application.Interfaces.Oportunidade
{
    public interface IOportunidadeReaderService
    {
        Task<Domain.Entities.Oportunidade.Oportunidade> GetOportunidadeByIdAsync(int id);
        Task<OportunidadePaginadoDTO> GetOportunidadesAsync(FilterOportunidadeDTO filtro);
        Task<GetOportunidadeDTO> GetOportunidadeByIdDetalhadoAsync(int id);
        Task<List<Domain.Entities.Oportunidade.Oportunidade>> GetListOportunidadesByLeadIdAsync(int leadId);
        Task<List<TipoInteresseDTO>> ListarTiposInteresseAsync();
        /// <summary>Oportunidades por período (criação ou modificação) para ETL. Inclui excluídos.</summary>
        Task<List<Domain.Entities.Oportunidade.Oportunidade>> ObterOportunidadesPorPeriodoParaETLAsync(DateTime dataInicio, DateTime dataFim);
        /// <summary>Oportunidades por lead para ETL. Inclui excluídos.</summary>
        Task<List<Domain.Entities.Oportunidade.Oportunidade>> ObterOportunidadesPorLeadIdParaETLAsync(int leadId);
        /// <summary>Oportunidades por lead evento para ETL. Inclui excluídos.</summary>
        Task<List<Domain.Entities.Oportunidade.Oportunidade>> ObterOportunidadesPorLeadEventoIdParaETLAsync(int leadEventoId);
        Task<List<Domain.Entities.Oportunidade.Oportunidade>> ObterOportunidadesPorIdsParaETLAsync(IEnumerable<int> oportunidadeIds);
        /// <summary>Nome do produto da oportunidade para ETL (ex.: produto de interesse no fato lead).</summary>
        Task<string?> ObterNomeProdutoOportunidadeParaETLAsync(int oportunidadeId);

        Task<List<EtapaHistorico>> ObterHistoricoEtapasPorOportunidadeIdAsync(int oportunidadeId);

        Task<Domain.Entities.Oportunidade.Oportunidade?> GetPrimeiraOportunidadeAsync(int leadId);

        /// <summary>Oportunidade por ID para ETL. Retorna null se não encontrada. Inclui excluídos.</summary>
        Task<Domain.Entities.Oportunidade.Oportunidade?> ObterOportunidadePorIdParaETLAsync(int id);
    }
}
