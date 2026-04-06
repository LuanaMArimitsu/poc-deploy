namespace WebsupplyConnect.Application.DTOs.Usuario
{
    public class DispositivoDetalheDTO
    {
        public int Id { get; set; }
        public string DeviceId { get; set; } = null!;
        public string Modelo { get; set; } = null!;
        public bool Ativo { get; set; }
        public bool Online { get; set; }
        public DateTime UltimaSincronizacao { get; set; }
        public string? SignalRConnectionId { get; set; }
        public DateTime? UltimoHeartbeatSignalR { get; set; }
        public DateTime? UltimaReconexao { get; set; }
        public DispositivoUsuarioDTO Usuario { get; set; } = null!;
        public DateTime DataCriacao { get; set; }
        public DateTime DataModificacao { get; set; }
    }

}
