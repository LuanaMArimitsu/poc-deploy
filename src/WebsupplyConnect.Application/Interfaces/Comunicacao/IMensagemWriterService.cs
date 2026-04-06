using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Domain.Entities.Comunicacao;

namespace WebsupplyConnect.Application.Interfaces.Comunicacao
{
    public interface IMensagemWriterService
    {
        Task ProcessarMensagemAsync(MensagemRequestDTO dto);
        Task<Mensagem> ProcessarStatusAsync(string messageMetaId, string statusMeta, long horaStatus, string? idConversaMeta);
        Task<string> MarcarMensagemComoLidaAsync(int conversaId);
        Task<int> UpdateStatusMensagensClienteAsync(int conversaId, int statusId);
        Task UpdateStatusMensagensAsync(int mensagemId, int statusId);
        Task UpdateIdMensagemMetaAsync(int mensagemId, string idMeta);
        Task<Mensagem> ProcessarMensagemMidiaAsync(string tipoMensagem, int conversaId, string messageMetaId, string midiaId);
        Task<Mensagem> ProcessarMensagemTextoAsync(string conteudoMensagem, string tipoMensagem, int conversaId, string messageMetaId);
        Task<Mensagem> ProcessarMensagemEnvioTemplateAsync(string tipoMensagem, int conversaId, int usuarioId, int templateId, string? idExternoMeta = null, List<string>? parametros = null);
        Task<Mensagem> ProcessarMensagemEnvioMidiaAsync(string conteudoMensagem, string tipoMensagem, int conversaId, int usuarioId);
        Task<Mensagem> ProcessarMensagemEnvioTextoAsync(string conteudoMensagem, string tipoMensagem, int conversaId, int usuarioResponsavelId, bool usouAssistenteIA, bool ehAviso);
        Task<Mensagem> ProcessarMensagemEnvioTemplateIntegracaoAsync(string tipoMensagem, int conversaId, int usuarioResponsavelId, Domain.Entities.Lead.Lead lead, Canal canal, int templateId, string? idExternoMeta);
    }
}
