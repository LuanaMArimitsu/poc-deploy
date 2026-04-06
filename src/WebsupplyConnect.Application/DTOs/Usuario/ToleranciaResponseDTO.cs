namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class ToleranciaResponseDTO
    {
        public int UsuarioId { get; set; }
        public bool Tolerancia { get; set; }
        public string Mensagem { get; set; } = string.Empty;
    }
}
