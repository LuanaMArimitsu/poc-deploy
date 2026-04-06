using WebsupplyConnect.Application.DTOs.Comunicacao;

namespace WebsupplyConnect.Application.Interfaces.ExternalServices
{
    public interface IChatBotClient
    {
        Task SendToChatBot(ChatMessageRequestDTO request, string urlChatBot);
    }
}
