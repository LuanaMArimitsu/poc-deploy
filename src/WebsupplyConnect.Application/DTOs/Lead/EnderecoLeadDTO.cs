using WebsupplyConnect.Domain.Entities.Lead;

namespace WebsupplyConnect.Application.DTOs.Lead
{
    public class EnderecoLeadDTO
    {
        public int LeadId { get; set; }
        public Endereco Endereco { get; set; } = null!;
        public bool IsComercial { get; set; } = false;

        public int ResponsavelId { get; set; }
        public int EmpresaId { get; set; }
    }
}