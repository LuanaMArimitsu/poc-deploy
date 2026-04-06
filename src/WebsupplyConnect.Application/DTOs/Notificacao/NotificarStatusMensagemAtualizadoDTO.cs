namespace WebsupplyConnect.Application.DTOs.Notificacao
{
    public record NotificarStatusMensagemAtualizadoDTO
    {
        public int UsuarioId { get; set; }
        public int MensagemId { get; set; }
        public string Status { get; set; } = string.Empty;
    }
}
