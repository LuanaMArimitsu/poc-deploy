namespace WebsupplyConnect.Application.DTOs.Lead.Campanha
{
    public class CriarCampanhaApiDTO
    {
        public string Nome { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public int EmpresaId { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public bool Temporaria { get; set; } = false;
        public int EquipeId { get; set; }
    }
}
