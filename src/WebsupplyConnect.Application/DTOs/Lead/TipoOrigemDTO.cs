namespace WebsupplyConnect.Application.DTOs.Lead
{
    public class TipoOrigemDTO
    {
        public int Id { get; set; }
        public string Codigo { get; set; } = string.Empty;
        public string Nome { get; set; } = string.Empty;
        public string Descricao { get; set; } = string.Empty;
        public int Ordem { get; set; }

    }
}
