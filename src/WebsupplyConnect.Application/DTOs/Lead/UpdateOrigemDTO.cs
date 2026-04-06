namespace WebsupplyConnect.Application.DTOs.Lead
{
    public class UpdateOrigemDTO
    {
        public string? Nome { get; set; }
        public string? Descricao { get; set; }
        public int? OrigemTipoId { get; set; }

        public int EmpresaId { get; set; }
    }
}
