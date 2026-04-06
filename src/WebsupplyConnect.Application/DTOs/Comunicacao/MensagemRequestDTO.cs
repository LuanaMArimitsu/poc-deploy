using Microsoft.AspNetCore.Http;

namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public class MensagemRequestDTO
    {
        public bool Midia { get; set; }
        public IFormFile? File { get; set; }
        public bool Template { get; set; }
        public int? TemplateId { get; set; }
        public string? Conteudo { get; set; } = string.Empty;
        public required string TipoMensagem { get; set; }
        public int LeadId { get; set; }
        public int UsuarioId { get; set; }
        public bool UsouAssistenteAi { get; set; }
        public bool? EhAviso { get; set; } = false;
    }
}
