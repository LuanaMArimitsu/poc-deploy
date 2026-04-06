using WebsupplyConnect.Application.DTOs;
using WebsupplyConnect.Application.DTOs.Lead;
using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Entities.Lead;

namespace WebsupplyConnect.Application.Interfaces.Lead
{
    public interface ILeadReaderService
    {
        Task<Domain.Entities.Lead.Lead> GetLeadByIdAsync(int id);
        Task<string?> GetLeadStatusCodigoAsync(int leadStatusId);
        Task<LeadRetornoDTO> GetDetalhesAsync(int id, int usuarioLogado);
        Task<bool> LeadExistsAsync(int id);


        Task<LeadPaginadoDTO> ListarLeadsAsync(LeadFiltroRequestDTO filtro, int usuarioIdLogado);
        Task<LeadPaginadoDTO> ListarLeadsNovoAsync(LeadFiltrosDto filtro, int usuarioIdLogado);
        Task<LeadPaginadoDTO> ListarLeadsPorPermissaoAsync(LeadFiltrosDto? filtro, int usuarioIdLogado);

        Task<List<StatusLeadDTO>> ListarStatusDoLeadAsync();
        Task<List<WebsupplyConnect.Domain.Entities.Lead.Lead>> ObterLeadsPendentesDistribuicaoAsync(int empresaId, int maxLeads);
        Task<int> CountLeadsDistribuidosAsync(int empresaId,
            DateTime? dataInicio = null,
            DateTime? dataFim = null);
        Task<bool> ExisteLeadAtribuidoAsync(int equipeId);
        Task<Domain.Entities.Lead.Lead> GetLeadComResponsavelAsync(int id);
        /// <summary>
        /// Obtém leads por IDs para uso em ETL/OLAP (ex.: listagem dashboard).
        /// </summary>
        Task<List<Domain.Entities.Lead.Lead>> ObterLeadsPorIdsAsync(IEnumerable<int> leadIds, bool includeDeleted = false);
        /// <summary>Leads com Responsavel e Usuario carregados (filtro OLAP: excluir atribuídos a bot).</summary>
        Task<List<Domain.Entities.Lead.Lead>> ObterLeadsComResponsavelUsuarioPorIdsAsync(
            IEnumerable<int> leadIds, bool includeDeleted = true);
        /// <summary>Lista entidades de status de lead para uso em ETL (sincronização dimensão status).</summary>
        Task<List<Domain.Entities.Lead.LeadStatus>> ListarStatusLeadEntidadesAsync();
        /// <summary>Lead com responsável para ETL. Inclui excluídos quando solicitado.</summary>
        Task<Domain.Entities.Lead.Lead?> ObterLeadComResponsavelParaETLAsync(int id, bool includeDeleted = true);
        /// <summary>Leads com data de modificação no período para ETL.</summary>
        Task<List<Domain.Entities.Lead.Lead>> ObterLeadsPorPeriodoModificacaoParaETLAsync(DateTime dataInicio, DateTime dataFim);
        Task<Domain.Entities.Lead.Lead> ObterLeadPorGrupoAsync(string? whatsAppNumero, string? email, string? cpf, int grupoEmpresaId);
        bool LeadPertenceAoBot(Domain.Entities.Lead.Lead lead);
        Task<LeadStatus?> GetLeadStatusByCodigo(string? codigo = null);
        string? NormalizarNumeroWhatsApp(string numero);
    }
}
