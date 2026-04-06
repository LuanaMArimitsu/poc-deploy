namespace WebsupplyConnect.Application.DTOs.Lead.Campanha
{
    public class CampanhaDTO
    {
        public int Id { get; set; }
        public string Nome { get; set; } = string.Empty;
        public string Codigo { get; set; } = string.Empty;
        public bool Ativo { get; set; }
        public bool Temporaria { get; set; }
        public int? IdTransferida { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int EmpresaId { get; set; }
        public int EquipeId { get; set; }
    }
}
