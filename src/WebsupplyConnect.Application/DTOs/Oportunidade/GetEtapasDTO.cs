namespace WebsupplyConnect.Application.DTOs.Oportunidade
{
    public class GetEtapasDTO
    {
        public int Id { get; set; }
        public required string Nome { get; set; }
        public required string Descricao { get; set; }
        public int Ordem { get; set; }
        public required string Cor { get; set; }
        public int ProbabilidadePadrao { get; set; }
        public bool EhAtiva { get; set; }
        public bool EhFinal { get; set; }
        public bool EhVitoria { get; set; }
        public bool EhPerdida { get; set; }
        public bool EhExibida { get; set; }
        public bool Ativo { get; set; }
        public int FunilId { get; set; }
    }
}
