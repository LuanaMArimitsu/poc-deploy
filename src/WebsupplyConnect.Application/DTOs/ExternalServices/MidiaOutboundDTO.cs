using System.Text.Json.Serialization;

namespace WebsupplyConnect.Application.DTOs.ExternalServices
{
    public class MidiaOutboundDTO
    {
        public required string BlobId { get; set; }
        public int MensagemId { get; set; }
        public int UsuarioId { get; set; }
        public int MidiaId { get; set; }
        public int CanalId { get; set; }
    }
}
