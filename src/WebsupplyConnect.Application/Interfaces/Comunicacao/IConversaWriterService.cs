using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Domain.Entities.Comunicacao;

namespace WebsupplyConnect.Application.Interfaces.Comunicacao
{
    public interface IConversaWriterService
    {
        Task<List<ConversaListaDTO>> ConversasSyncAsync(int usuarioId, int usuarioLogadoId);
        Task UpdateDataUltimaMensagemAsync(int conversaId, DateTime dataMensagem);
        Task UpdateConversaStatus(ConversaStatusDTO statusDTO, string commit);
        Task UpdateResponsavelAsync(Conversa conversa, int novoUsuarioId, int canalId, int equipeId);
        Task UpdatePossuiMensagensNaoLidas(int conversaId, bool possui);
        Task UpdateConversaMetaIdAsync(int conversaId, string idExternoMeta);
        Task<int> GetConversaByLeadAndCanalAsync(int leadId, int responsavelId, int canalId, string statusConversa, int equipeId, bool leadNovo, bool? integracao = false);
        Task<ConversaPag> GetConversasListaPaginadaAsync(int usuarioId, int usuarioLogadoId, ConversaPagParam param);
        Task UpdateInfoMensagemAsync(int conversaId, DateTime dataMensagem);
        Task EncerrarConversasAtivasByLeadAsync(int leadId);
        Task EncerrarConversaAsync(int conversaId, int usuarioLogado);
        Task EncerrarConversaAsync(int conversaId);
        Task<TemConversaDTO> VerificarSeLeadTemConversaAtivaAsync(int leadId, int usuarioLogadoId);
        Task<ConversasEncerradasLeadResultDTO> ListConversasEncerradasByLeadAsync(int leadId, LeadConversaEncerradaParamsDTO param);
        Task<string> AlterarFixacaoConversaAsync(int conversaId);
    }
}
