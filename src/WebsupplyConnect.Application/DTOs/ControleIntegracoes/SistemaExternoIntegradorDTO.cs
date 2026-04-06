namespace WebsupplyConnect.Application.DTOs.ControleIntegracoes
{
    public class SistemaExternoIntegradorDTO
    {
        public int Id { get; set; }
        public string URL_API { get; set; } = string.Empty;
        public string Token { get; set; } = string.Empty;
    }
}
