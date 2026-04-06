using WebsupplyConnect.Application.DTOs.Comunicacao;

namespace WebsupplyConnect.Application.DTOs.Notificacao
{
    public record NotificarNovaMensagemDTO
    {
        public int UsuarioId { get; set; }
        public int MensagemId { get; set; }
        public string Titulo { get; set; } = string.Empty;
        public required MensagemDTO MensagemSincronizacao { get; set; }
    }
}
