using WebsupplyConnect.Application.DTOs.Comunicacao;

namespace WebsupplyConnect.Application.DTOs.Notificacao
{
    public record NotificacaoDTO
    {
        public int Id { get; set; }
        public string Type { get; set; }
        public int ConversaID { get; set; }
        public int MensagemID { get; set; }
        public string Title { get; set; }
        public string? File { get; set; }
        public string? Content { get; set; }
        public string? Icon { get; set; }
        public string? Color { get; set; }
        public MensagemDTO? MensagemSincronizacao { get; set; }
    }
}
