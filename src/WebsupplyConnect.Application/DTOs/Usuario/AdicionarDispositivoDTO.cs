namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public record AdicionarDispositivoDTO
    {
        public int UsuarioId { get; set; }
        public string DeviceId { get; set; } 
        public string Modelo { get; set; }
    }
}
