using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Usuario
{
    /// <summary>
    /// Entidade que representa um dispositivo móvel associado a um usuário,
    /// utilizado para sincronização offline e notificações push.
    /// </summary>
    public class Dispositivo : EntidadeBase
    {
        /// <summary>
        /// ID do usuário proprietário do dispositivo
        /// </summary>
        public int UsuarioId { get; private set; }

        /// <summary>
        /// Identificador único do dispositivo no sistema
        /// </summary>
        public string DeviceId { get; private set; }

        /// <summary>
        /// Modelo/informações do dispositivo
        /// </summary>
        public string Modelo { get; private set; }

        /// <summary>
        /// Indica se o dispositivo está ativo
        /// </summary>
        public bool Ativo { get; private set; }

        /// <summary>
        /// Data e hora da última sincronização do dispositivo
        /// </summary>
        public DateTime UltimaSincronizacao { get; private set; }

        /// <summary>
        /// ID de conexão do SignalR para comunicação em tempo real
        /// </summary>
        public string? SignalRConnectionId { get; private set; }

        /// <summary>
        /// Data e hora do último heartbeat do SignalR
        /// </summary>
        public DateTime? UltimoHeartbeatSignalR { get; private set; }

        /// <summary>
        /// Data e hora da última reconexão do dispositivo
        /// </summary>
        public DateTime? UltimaReconexao { get; private set; }

        /// <summary>
        /// Navegação para o usuário
        /// </summary>
        public virtual Usuario Usuario { get; private set; }

        /// <summary>
        /// Construtor protegido para uso do EF Core
        /// </summary>
        protected Dispositivo()
        {
        }

        /// <summary>
        /// Construtor para criar um novo dispositivo
        /// </summary>
        /// <param name="usuarioId">ID do usuário proprietário</param>
        /// <param name="deviceId">Identificador único do dispositivo</param>
        /// <param name="modelo">Modelo/informações do dispositivo</param>
        public Dispositivo(int usuarioId, string deviceId, string modelo)
        {
            ValidarDominio(usuarioId, deviceId, modelo);

            UsuarioId = usuarioId;
            DeviceId = deviceId;
            Modelo = modelo;
            Ativo = true;
            UltimaSincronizacao = TimeHelper.GetBrasiliaTime();
        }

        /// <summary>
        /// Atualiza o modelo/informações do dispositivo
        /// </summary>
        /// <param name="modelo">Novo modelo/informações</param>
        public void AtualizarModelo(string modelo)
        {
            if (string.IsNullOrWhiteSpace(modelo))
                throw new DomainException("O modelo do dispositivo é obrigatório.", nameof(Dispositivo));

            if (modelo.Length > 200)
                throw new DomainException("O modelo do dispositivo não pode ter mais que 200 caracteres.", nameof(Dispositivo));

            Modelo = modelo;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Ativa o dispositivo
        /// </summary>
        public void Ativar()
        {
            Ativo = true;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Desativa o dispositivo
        /// </summary>
        public void Desativar()
        {
            Ativo = false;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Registra uma sincronização do dispositivo
        /// </summary>
        public void RegistrarSincronizacao()
        {
            UltimaSincronizacao = TimeHelper.GetBrasiliaTime();
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Atualiza o ID de conexão do SignalR
        /// </summary>
        /// <param name="connectionId">Novo ID de conexão</param>
        public void AtualizarSignalRConnectionId(string connectionId)
        {
            SignalRConnectionId = connectionId;
            UltimoHeartbeatSignalR = TimeHelper.GetBrasiliaTime();
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Registra um heartbeat do SignalR
        /// </summary>
        public void RegistrarHeartbeatSignalR()
        {
            UltimoHeartbeatSignalR = TimeHelper.GetBrasiliaTime();
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Registra uma reconexão do dispositivo
        /// </summary>
        public void RegistrarReconexao()
        {
            UltimaReconexao = DateTime.UtcNow;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Limpa os dados de conexão SignalR
        /// </summary>
        public void LimparConexaoSignalR()
        {
            SignalRConnectionId = null;
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Verifica se o dispositivo está conectado via SignalR
        /// </summary>
        /// <param name="timeoutMinutes">Tempo limite em minutos para considerar a conexão ativa</param>
        /// <returns>True se o dispositivo está conectado, False caso contrário</returns>
        public bool EstaConectadoViaSignalR(int timeoutMinutes = 5)
        {
            if (string.IsNullOrWhiteSpace(SignalRConnectionId) || !UltimoHeartbeatSignalR.HasValue)
                return false;

            var timeout = TimeSpan.FromMinutes(timeoutMinutes);
            return (DateTime.UtcNow - UltimoHeartbeatSignalR.Value) <= timeout;
        }

        /// <summary>
        /// Valida as regras de domínio para o dispositivo
        /// </summary>
        /// <param name="usuarioId">ID do usuário</param>
        /// <param name="deviceId">ID do dispositivo</param>
        /// <param name="modelo">Modelo do dispositivo</param>
        private void ValidarDominio(int usuarioId, string deviceId, string modelo)
        {
            if (usuarioId <= 0)
                throw new DomainException("O ID do usuário deve ser maior que zero.", nameof(Dispositivo));

            if (string.IsNullOrWhiteSpace(deviceId))
                throw new DomainException("O ID do dispositivo é obrigatório.", nameof(Dispositivo));

            if (deviceId.Length > 300)
                throw new DomainException("O ID do dispositivo não pode ter mais que 300 caracteres.", nameof(Dispositivo));

            if (string.IsNullOrWhiteSpace(modelo))
                throw new DomainException("O modelo do dispositivo é obrigatório.", nameof(Dispositivo));

            if (modelo.Length > 200)
                throw new DomainException("O modelo do dispositivo não pode ter mais que 200 caracteres.", nameof(Dispositivo));
        }
    }
}