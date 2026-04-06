using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.ExternalServices;
using WebsupplyConnect.Application.Interfaces.Comunicacao;
using WebsupplyConnect.Domain.Entities.Comunicacao;

namespace WebsupplyConnect.Application.Services.Comunicacao
{
    public class MensagemEnvioFilaFactory : IMensagemEnvioFilaFactory
    {
        public MensagemOutboundDTO CriarMensagemOutbound(Mensagem mensagem, MensagemRequestDTO? dto)
        {
            return new MensagemOutboundDTO
            {
                Id = mensagem.Id,
                ConversaId = mensagem.ConversaId,
                Conteudo = mensagem.Conteudo,
                UsuarioId = mensagem.UsuarioId,
                IdExternoMeta = null, // ainda será preenchido após envio à Meta
                StatusId = mensagem.StatusId,
                TipoId = mensagem.TipoId,
                MidiaId = mensagem?.Midia?.Id,
                TemplateId = dto?.TemplateId
            };
        }

        public MidiaOutboundDTO CriarMidiaOutbound(string blobId, int mensagemId, int usuarioID, int midiaId, int canalId)
        {
            return new MidiaOutboundDTO
            {
                BlobId = blobId,
                MensagemId = mensagemId,
                UsuarioId = usuarioID,
                MidiaId = midiaId,
                CanalId = canalId
            };
        }
    }
}
