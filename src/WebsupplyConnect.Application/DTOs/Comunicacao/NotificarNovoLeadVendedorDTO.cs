namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public class NotificarNovoLeadVendedorDTO
    {
        public int UsuarioId { get; set; }
        public int LeadId { get; set; }
        public required string NomeVendedor { get; set; }
    }
}
