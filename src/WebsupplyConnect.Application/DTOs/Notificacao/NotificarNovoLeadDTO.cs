namespace WebsupplyConnect.Application.DTOs.Notificacao
{
    public record NotificarNovoLeadDTO
    {
        public int UsuarioId { get; set; }
        public int LeadId { get; set; }
    }
}
