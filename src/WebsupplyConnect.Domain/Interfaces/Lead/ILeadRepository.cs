using System.Linq.Expressions;
using WebsupplyConnect.Domain.Entities.Lead;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Lead
{
    /// <summary>
    /// Interface de repositório para operações relacionadas a leads no sistema.
    /// </summary>
    public interface ILeadRepository : IBaseRepository
    {
        Task<List<Entities.Lead.Lead>> ListarLeadsAcompanhamentoEscopoAsync(
            int? usuarioId,
            List<int> empresaIds,
            List<int> equipeIds,
            List<int> origemIds,
            List<int> campanhaIds,
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            bool apenasPrimeiroAtendimentoAguardandoCliente = false);

        /// <summary>
        /// Mesmos filtros de <see cref="ListarLeadsAcompanhamentoEscopoAsync"/>, retornando apenas IDs ordenados
        /// (fase 1 da paginação de pendentes: escopo leve no banco).
        /// </summary>
        Task<List<int>> ListarIdsAcompanhamentoEscopoAsync(
            int? usuarioId,
            List<int> empresaIds,
            List<int> equipeIds,
            List<int> origemIds,
            List<int> campanhaIds,
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            bool apenasPrimeiroAtendimentoAguardandoCliente = false);

        /// <summary>
        /// Carrega dados mínimos para <c>AvaliarLeadPendente</c> (conversas ativas + mensagens hidratadas).
        /// </summary>
        Task<List<Entities.Lead.Lead>> CarregarLeadsAvaliacaoAcompanhamentoPorIdsAsync(
            List<int> leadIds,
            int statusConversaEncerradaId);

        Task<List<Entities.Lead.Lead>> ListarLeadsAcompanhamentoEscopoParaPendenciaAsync(
            int? usuarioId,
            List<int> empresaIds,
            List<int> equipeIds,
            List<int> origemIds,
            List<int> campanhaIds,
            List<int> vendedorUsuarioIds,
            List<int> statusLeadIds);

        Task<List<Entities.Lead.Lead>> ObterLeadsDetalhesDashboardPorIdsAsync(List<int> leadIds);

        Task<Domain.Entities.Lead.Lead?> GetLeadWithDetailsAsync(int id, bool includeDeleted = false);

        Task<Domain.Entities.Lead.Lead?> GetLeadByWhatsAppNumberAndGroupAsync(
            string whatsAppNumber,
            List<int> empresaIds);
        Task<Domain.Entities.Lead.Lead?> GetLeadWithUsuarioIncludingDeletedAsync(int id);
        Task<Domain.Entities.Lead.Lead?> ObterLeadExistenteNoMesmoGrupo(string? whatsAppNumero, string? email, string? cpf, int grupoEmpresaId);

        /// <summary>
        /// Obtém um lead existente a partir do número de WhatsApp informado.
        /// </summary>
        /// <param name="whatsAppNumber">Número de WhatsApp a ser consultado.</param>
        /// <param name="empresaId">ID da empresa para a qual o lead pertence.</param>
        /// <returns>Retorna o Lead correspondente ou null se não for encontrado.</returns>
        Task<Entities.Lead.Lead?> GetLeadByWhatsAppNumberAsync(string whatsAppNumber, int empresaId);

        /// <summary>
        /// Obtém o ID do status de lead correspondente ao código informado (case-insensitive),
        /// ignorando registros marcados como excluídos.
        /// </summary>
        /// <param name="status">O código do status a ser buscado.</param>
        /// <returns>O ID do status de lead, ou 0 se não encontrado.</returns>
        Task<int> GetLeadStatusId(string status);
        Task<Domain.Entities.Lead.Lead?> GetLeadWithUsuarioAsync(int id);

        /// <summary>
        /// Obtém o lead com Responsavel e Usuario incluídos, para uso no ETL/OLAP.
        /// </summary>
        Task<Domain.Entities.Lead.Lead?> GetLeadComResponsavelAsync(int id, bool includeDeleted = false);

        /// <summary>
        /// Obtém vários leads com Responsavel e Usuario (para filtrar atribuição a bot no OLAP).
        /// </summary>
        Task<List<Domain.Entities.Lead.Lead>> GetLeadsComResponsavelUsuarioPorIdsAsync(
            IEnumerable<int> leadIds, bool includeDeleted = false);

        Task<string?> GetLeadStatusCodigoAsync(int leadStatusId, bool includeDeleted = false);

        /// <summary>
        /// Conta o número de leads recebidos por um vendedor em um período específico
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias para a contagem</param>
        /// <returns>Número de leads recebidos</returns>
        Task<int> ContarLeadsRecebidosPorVendedorAsync(int vendedorId, int empresaId, int periodoEmDias = 30);

        /// <summary>
        /// Conta o número de leads convertidos por um vendedor em um período específico
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias para a contagem</param>
        /// <returns>Número de leads convertidos</returns>
        Task<int> ContarLeadsConvertidosPorVendedorAsync(int vendedorId, int empresaId, int periodoEmDias = 30);

        /// <summary>
        /// Conta o número de leads perdidos por inatividade por um vendedor em um período específico
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias para a contagem</param>
        /// <returns>Número de leads perdidos por inatividade</returns>
        Task<int> ContarLeadsPerdidosPorInatividadeAsync(int vendedorId, int empresaId, int periodoEmDias = 30);

        /// <summary>
        /// Calcula a velocidade média de atendimento de um vendedor em minutos
        /// </summary>
        /// <param name="vendedorId">ID do vendedor</param>
        /// <param name="empresaId">ID da empresa</param>
        /// <param name="periodoEmDias">Período em dias para o cálculo</param>
        /// <returns>Velocidade média em minutos</returns>
        Task<decimal> CalcularVelocidadeMediaAtendimentoAsync(int vendedorId, int empresaId, int periodoEmDias = 30);

        Task<(List<Entities.Lead.Lead> Itens, int TotalItens)> ListarLeadsFiltradoAsync(
            int? origemId,
            int? statusId,
            int? usuarioId,
            DateTime? dataCadastroInicio,
            DateTime? dataCadastroFim,
            string? nivelInteresse,
            int? pagina,
            int? tamanhoPagina,
            string? busca,
            int? empresaId,
            string? numeroWhatsapp
        );

        Task<List<LeadStatus>> ListarStatusAsync();
        Task<List<Entities.Lead.Lead>> ObterLeadsPendentesDistribuicaoAsync(int empresaId, int maxLeads);
        Task<int> CountLeadsDistribuidosAsync(int empresaId, DateTime? dataInicio = null, DateTime? dataFim = null);
        Task<bool> ExisteLeadAtribuidoAsync(int equipeId, int? membroId = null);
        Task<List<Entities.Lead.Lead>> GetListLeadExportAsync(int empresaId, int? equipeId, int? usuarioId, int? statusId, DateTime? de, DateTime? ate);
        Task<Entities.Lead.Lead?> GetLeadsComResponsavelAsync(int id);

        Task<(List<Entities.Lead.Lead> Itens, int TotalItens)> ListarLeadsFiltradoAsync(
            int? leadId,
            List<int>? empresasId,
            int? equipeId,
            int? usuarioIdLogado,
            bool meusLeads,
            List<int>? responsavelIds,
            bool? comOportunidades,
            List<int>? statusIds,
            List<int>? origemIds,
            DateTime? dataInicio,
            DateTime? dataFim,
            bool? comConversasAtivas,
            bool? comMensagensNaoLidas,
            bool? aguardandoResposta,
            string? whatsApp,
            string? email,
            string? cpf,
            string? textoBusca,
            int? pagina,
            int? tamanhoPagina,
            string orderBy,
            int statusEncerrado);
    }
}
