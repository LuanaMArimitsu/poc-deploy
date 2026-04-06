using WebsupplyConnect.Application.DTOs.Comunicacao;
using WebsupplyConnect.Application.DTOs.ExternalServices;
using WebsupplyConnect.Domain.Entities.Comunicacao;

namespace WebsupplyConnect.Application.Interfaces.Comunicacao
{
    public interface IMensagemEnvioFilaFactory
    {
        MensagemOutboundDTO CriarMensagemOutbound(Mensagem mensagem, MensagemRequestDTO? dto);
        MidiaOutboundDTO CriarMidiaOutbound(string blobId, int mensagemId, int usuarioID, int midiaId, int canalId);
    }
}
