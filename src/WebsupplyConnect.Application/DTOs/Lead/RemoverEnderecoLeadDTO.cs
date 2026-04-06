namespace WebsupplyConnect.Application.DTOs.Lead
{
    public class RemoverEnderecoLeadDTO
    {
        public int LeadId { get; set; }
        public int EnderecoId { get; set; }

        public int ResponsavelId { get; set; }
        public int EmpresaId { get; set; }
    }
}