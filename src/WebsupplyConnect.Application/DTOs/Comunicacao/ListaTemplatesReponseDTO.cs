namespace WebsupplyConnect.Application.DTOs.Comunicacao
{
    public class ListaTemplatesReponseDTO
    {
        public required string Nome {  get; set; }
        public required string Conteudo { get; set; }
        public required string Descricao { get; set; }
        public int Id { get; set; }
    }
}
