using WebsupplyConnect.Domain.Entities.Comunicacao;
using WebsupplyConnect.Domain.Interfaces.Base;

namespace WebsupplyConnect.Domain.Interfaces.Comunicacao
{
    public interface IMensagemRepository : IBaseRepository
    {
        Task<int> GetMensagemStatus(string codigo, bool includeDeleted = false);
        Task<int> GetMensagemTipo(string codigo, bool includeDeleted = false);
        //Task<List<Mensagem>> GetMessagesFromDateForSync(DateTime? dataUltimaMensagem, int conversaId);
        Task<List<Mensagem>> GetOldMessages(DateTime dataEnvio, int conversaId, int? pageSize = null);
        Task<List<MensagemTipo>> GetCodigoMessagesSync();
        Task<List<Mensagem>> GetMensagensNaoLidasByConversaAsync(int conversaId, int statusId);
        Task<Mensagem?> GetUltimaMensagemByConversaAsync(int conversaId, bool includeDeleted = false);
        Task<int> UpdateStatusMensagensClienteAsync(int conversaId, int novoStatusId);
        Task<Mensagem?> GetUltimaMensagemByConversaIdAsync(int conversaId);

        Task<List<Mensagem>> GetMessagesFromDateForSync(
                    int conversaId,
                    int? quantidadeInicio,
                    int? quantidadeFim,
                    DateTime? dataUltimaMensagem = null,
                    bool? includeEhAviso = true);

        Task<int?> GetQntdMensagensNaoLidasByConversaAsync(int conversaId, int statusId);
        Task<Dictionary<int, Mensagem?>> GetUltimasMensagensByListConversasAsync(List<int> conversaIds);
    }
}
