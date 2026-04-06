namespace WebsupplyConnect.Application.DTOs.Lead
{
    public class LeadExportEmailDTO
    {
        public int EmpresaId { get; set; }
        public int? EquipeId { get; set; }
        public int? UsuarioId { get; set; }
        public int? StatusId { get; set; }
        public DateTime? De { get; set; }
        public DateTime? Ate { get; set; }
    }
}
