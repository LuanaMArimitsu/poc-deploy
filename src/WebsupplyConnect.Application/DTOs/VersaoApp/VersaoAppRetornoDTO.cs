namespace WebsupplyConnect.Application.DTOs.VersaoApp
{
    public class VersaoAppRetornoDTO
    {
        public string Versao { get; set; }
        public string? PlataformaApp { get; set; }
        public bool AtualizacaoObrigatoria { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
