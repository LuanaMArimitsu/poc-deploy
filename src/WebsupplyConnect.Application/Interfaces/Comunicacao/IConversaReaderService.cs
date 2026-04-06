using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Domain.Entities.Comunicacao;

namespace WebsupplyConnect.Application.Interfaces.Comunicacao
{
    public interface IConversaReaderService
    {
        /// <summary>
        /// Busca uma conversa existente através do Id.
        /// <param name="conversaId">Id da conversa sendo buscada. </param>
        /// <returns>Retorna a conversa buscada a partir de seu id.</returns>
        Task<Conversa> GetConversaByIdAsync(int conversaId);
        Task<Conversa?> GetConversaByLead(int id, string statusEncerrado);
        Task<List<Conversa>> GetAllConversasAtivaByLeadAsync(int id, string statusEncerrado);
        Task<List<Conversa>> GetConversasByUsuarioAsync(int usuarioId, string codigoStatus);
        Task<ConversaStatus> GetConversaStatusAsync(int? id = null, string? codigo = null);
        Task<List<ConversaStatus>> GetListConversaStatus();
        Task<bool> IsPrimeiraMensagemCliente(int conversaId);
        Task<bool> ExisteConversaNoCanalAsync(int usuarioId, int canalId);
        Task<ConversasEncerradasResultDTO> ListConversasEncerradaAsync(int usuarioId, ConversaPagParam param);
        Task<List<Conversa>> GetConversasComInatividade(int responsavelId, int pagina, int tamanhoPagina);
        Task<List<Conversa>> GetConversasComAviso(int responsavelId, int pagina, int tamanhoPagina);
        Task<Conversa?> GetUltimaConversaLead(int leadId, int equipeId);
        Task<List<Conversa>> GetConversasSemAtendimento(int pagina, int tamanhoPagina);
        /// <summary>Conversas do lead para ETL (contagem de conversas/mensagens). Inclui excluídos.</summary>
        Task<List<Conversa>> ObterConversasPorLeadIdParaETLAsync(int leadId);

        /// <summary>Conversas modificadas no período para identificação de leads afetados no ETL.</summary>
        Task<List<Conversa>> ObterConversasPorPeriodoModificacaoParaETLAsync(DateTime dataInicio, DateTime dataFim);

        Task<List<Conversa>> GetAllConversasByLeadAsync(int leadId);

        /// <summary>
        /// Retorna um dicionário LeadId → ConversaId para as conversas ativas (não encerradas) dos leads informados.
        /// Se o lead tiver mais de uma conversa ativa, retorna a mais recente (maior Id).
        /// </summary>
        Task<Dictionary<int, int>> ObterConversaAtivaIdsPorLeadIdsAsync(List<int> leadIds);
        Task<bool> ExisteConversaEncerradaPorLeadAsync(int leadId);
        Task<Dictionary<int, (string? Contexto, DateTime? DataAtualizacaoContexto, bool TrocaDeContato, string? ClassificacaoIA)>> GetContextosByIdsAsync(IReadOnlyCollection<int> conversaIds);

    }
}
