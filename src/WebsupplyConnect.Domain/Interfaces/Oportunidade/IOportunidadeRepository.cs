using WebsupplyConnect.Domain.Entities.Oportunidade;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Oportunidade
{
    public interface IOportunidadeRepository : IBaseRepository
    {
       Task<(List<Entities.Oportunidade.Oportunidade> itens, int totalItens)> ListarOportunidadesFiltradoAsync(int? leadId, int? produtoId, int? etapaId, decimal? valorMinimo, decimal? valorMaximo, int? responsavelId, int? origemId, int? empresaId, DateTime? dataPrevisaoFechamento, int? pagina, int? tamanho, DateTime? de, DateTime? ate);

        Task<Entities.Oportunidade.Oportunidade?> GetDetailsById(int id);

        Task<List<TipoInteresse>> ListarTiposInteresseAsync();

        /// <summary>
        /// Obtém o histórico de etapas de uma oportunidade, ordenado por data de mudança (mais recente primeiro).
        /// </summary>
        Task<List<EtapaHistorico>> ObterHistoricoEtapasPorOportunidadeIdAsync(int oportunidadeId, CancellationToken cancellationToken = default);
    }
}
