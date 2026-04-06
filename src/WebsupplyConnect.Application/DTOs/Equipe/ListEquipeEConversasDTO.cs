namespace WebsupplyConnect.Application.DTOs.Equipe
{
    public class ListEquipeEConversasDTO
    {
        public int EmpresaId { get; set; }
        public string EmpresaNome { get; set; }
        public int EquipeId { get; set; }
        public string Nome { get; set; } = string.Empty;
        public int QuantidadeConversasAtivas { get; set; }
    }
}
