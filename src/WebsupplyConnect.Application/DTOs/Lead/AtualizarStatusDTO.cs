namespace WebsupplyConnect.Application.DTOs.Lead
{
    public class AtualizarStatusDTO
    {
        public int StatusId { get; set; }
        public string? Observacao { get; set; }
        public int ResponsavelId { get; set; }
        public int EmpresaId { get; set; }
    }

}
