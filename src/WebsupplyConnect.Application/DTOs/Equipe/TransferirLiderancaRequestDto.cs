namespace WebsupplyConnect.Application.DTOs.Equipe
{
    public class TransferirLiderancaRequestDto
    {
        public int EquipeId { get; set; }
        public int NovoResponsavelMembroId { get; set; }
        public int EmpresaId { get; set; }
    }
}
