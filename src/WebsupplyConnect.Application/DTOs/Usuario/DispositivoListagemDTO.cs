namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class DispositivoListagemDTO
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public string Modelo { get; set; }
        public int UsuarioId { get; set; }
        public string UsuarioNome { get; set; }
        public string UsuarioEmail { get; set; }
        public bool Ativo { get; set; }
        public bool Online { get; set; }
        public DateTime? UltimaSincronizacao { get; set; }
        public string? SignalRConnectionId { get; set; }
        public DateTime? UltimoHeartbeatSignalR { get; set; }
        public DateTime DataCriacao { get; set; }
    }
}
