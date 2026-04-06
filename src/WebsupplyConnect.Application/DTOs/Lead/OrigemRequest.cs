namespace WebsupplyConnect.Application.DTOs.Lead
{
    public class OrigemRequest
    {
        public string Nome { get; set; } = string.Empty;
        public int OrigemTipoId { get; set; }
        public string? Descricao { get; set; }

        public int EmpresaId { get; set; }
    }
}
