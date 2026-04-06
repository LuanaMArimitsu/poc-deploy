namespace WebsupplyConnect.Application.DTOs.Equipe
{
    public class TipoEquipeDto
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string? Descricao { get; set; }
        public int Ordem { get; set; }
        public string? Icone { get; set; }
    }
}
