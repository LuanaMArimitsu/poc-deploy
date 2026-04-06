using System.Collections.Concurrent;
using WebsupplyConnect.Application.Interfaces.ExternalServices;

namespace WebsupplyConnect.Infrastructure.ExternalServices.SignalR
{
    // Classe responsável por gerenciar conexões SignalR dos usuários
    public class SignalRConnection : ISignalRConnection
    {
        // Mapeia o UserId para uma lista de ConnectionIds (um usuário pode ter várias conexões)
        private readonly ConcurrentDictionary<int, HashSet<string>> _userConnections = new();

        // Mapeamento reverso: ConnectionId -> UserId (útil para remover a conexão rapidamente)
        private readonly ConcurrentDictionary<string, int> _connectionToUser = new();

        // Armazena a última atividade de cada conexão
        private readonly ConcurrentDictionary<string, DateTime> _connectionActivity = new();

        /// <summary>
        /// Adiciona uma nova conexão ao usuário.
        /// Atualiza os dicionários de conexões e a última atividade.
        /// </summary>
        public void AddConnection(int userId, string connectionId)
        {
            _userConnections.AddOrUpdate(userId,
                new HashSet<string> { connectionId },
                (key, connections) =>
                {
                    lock (connections)
                    {
                        connections.Add(connectionId);
                    }
                    return connections;
                });

            _connectionToUser[connectionId] = userId;

            // Define a última atividade como agora
            UpdateConnectionActivity(userId, connectionId);
        }

        /// <summary>
        /// Remove uma conexão específica do usuário.
        /// Remove o usuário do dicionário se não houver mais conexões.
        /// </summary>
        public void RemoveConnection(int userId, string connectionId)
        {
            if (_userConnections.TryGetValue(userId, out var connections))
            {
                connections.Remove(connectionId);

                if (!connections.Any())
                {
                    _userConnections.TryRemove(userId, out _);
                }
            }

            // Sempre limpa os rastros da conexão removida
            _connectionToUser.TryRemove(connectionId, out _);
            _connectionActivity.TryRemove(connectionId, out _);
        }


        /// <summary>
        /// Verifica se o usuário está online.
        /// Considera online se houver pelo menos uma conexão ativa nos últimos 2 minutos.
        /// </summary>
        public bool IsUserOnline(int userId)
        {
            if (!_userConnections.TryGetValue(userId, out var connections))
                return false;

            lock (connections)
            {
                var cutoffTime = DateTime.UtcNow.AddMinutes(-2);

                return connections.Any(connId =>
                    _connectionActivity.TryGetValue(connId, out var lastActivity) &&
                    lastActivity > cutoffTime);
            }
        }

        /// <summary>
        /// Atualiza o horário da última atividade da conexão.
        /// </summary>
        public void UpdateConnectionActivity(int userId, string connectionId)
        {
            if (_connectionToUser.ContainsKey(connectionId))
            {
                _connectionActivity[connectionId] = DateTime.UtcNow;
            }
        }
    }
}
