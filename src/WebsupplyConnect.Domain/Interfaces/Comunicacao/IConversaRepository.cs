using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Entities.Empresa;
using WebsupplyConnect.Domain.Interfaces.Base;


namespace WebsupplyConnect.Domain.Interfaces.Comunicacao
{
    public interface IConversaRepository : IBaseRepository
    {
        Task<Conversa> GetConversaById(int conversaId, bool includeDeleted = false);
        /// <summary>
        /// Busca o status da convesa pelo código
        /// </summary>
        /// <param name="codigo">Código do status</param>
        /// <param name="includeDeleted">Se deve incluir status excluídos</param>
        /// <returns>O Id do status código.</returns>
        Task<int> GetConversaStatusByCodeAsync(string codigo, bool includeDeleted = false);


        /// <summary>
        /// Busca a conversa através do leadId e canalId, excluindo um status específico.
        /// </summary>
        /// <param name="leadId">LeadId que pertence a conversa.</param>
        /// <param name="canalId">CanalId que a conversa pertence.</param>
        /// <param name="statusIdExcluir">StatusId que deve ser excluído da busca (ex.: ENCERRADA).</param>
        /// <param name="includeDeleted">Se deve incluir status excluídos</param>
        /// <returns>A conversa encontrada no canal para o lead, diferente do status excluído.</returns>
        Task<Conversa?> GetConversaByLeadAndCanalAsync(int leadId, int canalId, int statusIdExcluir, bool includeDeleted = false);
        Task<List<Conversa>> GetConversasByUsuarioAsync(int usuarioId, int statusIdExcluir);
        Task AtualizarAsync(Conversa conversa);
        Task<bool> IsPrimeiraMensagemClienteAsync(int conversaId);
        Task<bool> ExisteConversaNoCanalAsync(int usuarioId, int canalId, int statusEncerrado);
        Task<Mensagem> GetPrimeiraMensagemClienteAsync(int conversaId);
        Task<List<Conversa>> GetConversasEncerradasByUsuarioAsync(int usuarioId, int statusEncerradoId, int? indiceInicial = null, int? indiceFinal = null, int? empresaId = null);
        Task<int> GetTotalConversasEncerradasByUsuarioAsync(int usuarioId, int statusEncerradoId);
        Task<(List<Conversa> conversas, int total)> GetConversasPaginadasByUsuarioAsync(int usuarioId, int statusEncerradoId, int? quantidadeInicial, int? quantidadeFinal,int? empresaId = null, int? equipeId = null);
        Task<List<Conversa>> GetConversasComInatividade(int responsavelId, int pagina, int tamanhoPagina);
        Task<List<Conversa>> GetConversasComAviso(int responsavelId, int pagina, int tamanhoPagina);
        Task<Conversa?> GetConversaNaoEncerradasByLeadAAsync(int leadId, int statusEncerrado);
        Task<List<Conversa>> GetConversasSemAtendimento(int pagina, int tamanhoPagina);
        Task<Dictionary<int, (string? Contexto, DateTime? DataAtualizacaoContexto, bool TrocaDeContato, string? ClassificacaoIA)>> GetContextosByIdsAsync(IReadOnlyCollection<int> conversaIds);
        Task AtualizarContextoAsync(int conversaId, string? contexto, DateTime dataAtualizacaoContexto);
        Task AtualizarClassificacaoAsync(int conversaId, bool trocaDeContato, string? classificacaoIA, DateTime dataAtualizacao);

        Task<Conversa?> GetUltimaConversaLead(int lead, int equipeId);
        Task<(List<Conversa> Conversas, int Total)> GetConversasEncerradasByLeadAsync(int leadId, int statusEncerradoId, int? pagInicial = null, int? pagFinal = null);
        Task<bool> ExisteConversaEncerradaPorLeadAsync(int leadId);
        Task<Conversa?> GetConversaParaClassificacaoPorIdAsync(int conversaId);
        Task<bool> JanelaAbertaDaConversaAsync(int conversaId, int statusEncerrada);
        Task<int> GetQuantidadeConversasFixadasAsync(int conversaId, int usuarioId);
    }
}
