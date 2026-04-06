using Microsoft.AspNetCore.Http;

namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public class MensagemDTO
    {
        public required int MensagemId { get; set; }
        public bool Midia { get; set; }
        public string? File { get; set; }
        public int? MidiaId { get; set; }
        public bool Template { get; set; }
        public int? TemplateId { get; set; }
        public string? Conteudo { get; set; } = string.Empty;
        public required string TipoMensagem { get; set; }
        public string MensagemStatus { get; set; } = string.Empty;
        public DateTime DataEnvio { get; set; }
        public char TipoRemetente { get; set; }
        public int LeadId { get; set; }
        public int UsuarioId { get; set; }
    }
}