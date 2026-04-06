namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    /// <summary>
    /// DTO para buscar uma conversa.
    /// </summary>
    public class ConversaGetDTO
    {
        public int LeadId { get; set; }
        public int CanalId { get; set; }
        public int StatusId { get; set; }
        public int UsuarioId { get; set; }
        public DateTime DataInicio { get; set; }

    }
}
