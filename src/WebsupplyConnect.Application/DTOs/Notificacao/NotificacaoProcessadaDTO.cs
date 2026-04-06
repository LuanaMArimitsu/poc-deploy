namespace WebsupplyConnect.Application.DTOs.Notificacao
{
    public class NotificacaoProcessadaDTO
    {
        public int Id { get; set; }
        public string Tipo { get; set; } = string.Empty;
        public DateTime DataCriacao { get; set; }
    }
}
