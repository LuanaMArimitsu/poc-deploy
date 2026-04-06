namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class DispositivoDTO
    {
        public int Id { get; set; }
        public string DeviceId { get; set; }
        public string Modelo { get; set; }
        public bool Ativo { get; set; }
        public DateTime UltimaSincronizacao { get; set; }
        public bool Online { get; set; }
        public string SignalRConnectionId { get; set; }
        public DateTime? UltimoHeartbeatSignalR { get; set; }
    }
}
