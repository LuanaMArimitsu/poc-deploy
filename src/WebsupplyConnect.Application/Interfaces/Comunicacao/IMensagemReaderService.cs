using System.Threading.Tasks;
using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Domain.Entities.Comunicacao;

namespace WebsupplyConnect.Application.Interfaces.Comunicacao
{
    public interface IMensagemReaderService
    {
        Task<bool> MensagemExistsAsync(int id);
        Task<List<MensagemStatus>> GetListMensagemStatusAsync();
        Task<List<MensagemTipo>> GetListMensagemTiposAsync();
        Task<MensagemStatus> GetMensagensStatusAsync(int? id, string? codigo);
        Task<MensagemTipo> GetMensagemTipoAsync(int id);
        Task<MensagemTipo> GetMensagemTipoByCodigoAsync(string codigo);
        Task<Mensagem> GetMensagemByIdAsync(int id);
        Task<Mensagem> GetMensagemByIdMeta(string idMeta);
        Task<MensagemStatus> GetMensagemStatusByCodigo(string codigo);
        Task<Mensagem?> GetUltimaMensagemByConversaIdAsync(int conversaId);
        Task<List<MensagemDTO>> GetMensagensRecentesAsync(int conversaId, int? quantidadeInicio, int? quantidadeFim, DateTime? dataInicio = null);
        Task<List<MensagemDTO>> GetMensagensAntigasAsync(DateTime dataLimite, int conversaId, int? pageSize);
        Task<List<Mensagem>> GetMensagensNaoLidasByConversaAsync(int conversaId, int statusId);
        Task<int?> GetQntdMensagensNaoLidasByConversaAsync(int conversaId, string status);
        Task<Mensagem?> GetUltimaMensagemByConversaAsync(int conversaId, bool includeDeleted = false);
        Task<List<MensagemDTO>> GetMensagensRecentesSemAviso(int conversaId, int? quantidadeInicio, int? quantidadeFim);
        Task<List<MensagemDTO>> GetTodasMensagens(int conversaId);
        /// <summary>Mensagens da conversa para ETL (contagem, status). Inclui excluídos.</summary>
        Task<List<Mensagem>> ObterMensagensPorConversaIdParaETLAsync(int conversaId, char? sentido = null);

        /// <summary>LeadIds com mensagens criadas/modificadas no período (via join com Conversa).</summary>
        Task<List<int>> ObterLeadIdsComMensagensNoPeriodoParaETLAsync(DateTime dataInicio, DateTime dataFim);
        Task<Dictionary<int, Mensagem?>> GetUltimasMensagensByListConversasAsync(List<int> conversaIds);
    }
}
