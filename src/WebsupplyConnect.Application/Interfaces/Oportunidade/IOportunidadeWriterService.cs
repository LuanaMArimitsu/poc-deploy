using WebsupplyConnect.Application.DTOs.Oportunidade;

namespace WebsupplyConnect.Application.Interfaces.Oportunidade
{
    public interface IOportunidadeWriterService
    {
        Task<Domain.Entities.Oportunidade.Oportunidade> CreateOportunidadeAsync(CreateOportunidadeDTO dto);
        Task UpdateResponsavelOportunidade(Domain.Entities.Oportunidade.Oportunidade oportunidade, int novoResponsavelId, int empresaId);
        Task UpdateEtapaOpotunidade(int oportunidadeId, ChangeEtapaDTO dto);
        Task UpdateOportunidadeAsync(UpdateOportunidadeDTO dto);
        Task DeleteOportunidadeAsync(int id);
        Task EnviarParaIntegrador(int oportunidadeId, int usuarioLogado);
        Task AtualizarConversaoAsync(ConversaoOportunidadeDTO request);
    }
}
