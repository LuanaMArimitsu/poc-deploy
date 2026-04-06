namespace WebsupplyConnect.Application.DTOs.ExternalServices
{
    public class MensagemOutboundDTO
    {
        public int Id { get; set; }
        public int ConversaId { get; set; }
        public string Conteudo { get; set; } = string.Empty;
        public int? UsuarioId { get; set; }
        public string? IdExternoMeta { get; set; }
        public int? StatusId { get; set; }
        public int TipoId { get; set; }
        public int? MidiaId { get; set; }
        public int? TemplateId { get; set; }
    }
}
