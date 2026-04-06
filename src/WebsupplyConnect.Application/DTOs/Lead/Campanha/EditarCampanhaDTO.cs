namespace WebsupplyConnect.Application.DTOs.Lead.Campanha
{
    public class EditarCampanhaDTO
    {
        public string Nome { get; set; }
        public string Codigo { get; set; }
        public bool Ativo { get; set; }
        public bool Temporaria { get; set; }
        public DateTime? DataInicio { get; set; }
        public DateTime? DataFim { get; set; }
        public int EmpresaId { get; set; }
        public int EquipeId { get; set; }
    }
}