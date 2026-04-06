using WebsupplyConnect.Application.DTOs.Comunicacao;

namespace WebsupplyConnect.Application.Interfaces.Comunicacao
{
    public interface IChatBotWriterService
    {
        Task<bool> CreateHistoryToBot(CreateHistoryBotObjectDTO botObject);
        Task<CreateHistoryToBotDTO?> GetHistoryToBot(int conversaId);
        Task UpdateHistoryBot(MessageRedisDTO novaMensagem, int conversaId);
    }
}
