namespace WebsupplyConnect.Application.DTOs.Lead
{
    public class OrigemDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public int OrigemTipoId { get; set; }
        public string OrigemTipoNome { get; set; } = string.Empty;
    }
}