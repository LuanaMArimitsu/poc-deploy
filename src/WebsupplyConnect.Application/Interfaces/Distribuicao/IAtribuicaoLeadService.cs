using WebsupplyConnect.Domain.Entities.Distribuicao;

namespace WebsupplyConnect.Application.Interfaces.Distribuicao
{
    public interface IAtribuicaoLeadService
    {
        Task<AtribuicaoLead> CriarAtribuicaoAsync(AtribuicaoLead atribuicao);
        Task<AtribuicaoLead> UpdateAsync(AtribuicaoLead atribuicao);
        Task<AtribuicaoLead?> ObterUltimaAtribuicaoLeadAsync(int leadId);
        Task<List<AtribuicaoLead>> ListAtribuicoesPorLeadAsync(int leadId);
        Task<bool> LeadPossuiResponsavelAsync(int leadId);

        Task<List<AtribuicaoLead>> ListAtribuicoesPorVendedorAsync(
            int vendedorId,
            int empresaId,
            DateTime? dataInicio = null,
            DateTime? dataFim = null,
            int pagina = 1,
            int tamanhoPagina = 20);

        Task<int> CountAtribuicoesPorVendedorAsync(
            int vendedorId,
            int empresaId,
            DateTime? dataInicio = null,
            DateTime? dataFim = null);

        Task<List<AtribuicaoLead>> ListAtribuicoesPorEmpresaAsync(
            int empresaId,
            DateTime? dataInicio = null,
            DateTime? dataFim = null);

        Task<List<object>> GetDistribuicoesPorVendedorAsync(
            int empresaId,
            DateTime? dataInicio = null,
            DateTime? dataFim = null);
    }
}
