using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Exceptions;
using WebsupplyConnect.Domain.Helpers;

namespace WebsupplyConnect.Domain.Entities.Distribuicao
{
    /// <summary>
    /// Registra o histórico de execuções do sistema de distribuição.
    /// Permite análise de performance e auditoria das distribuições realizadas.
    /// </summary>
    public class HistoricoDistribuicao : EntidadeBase
    {
        /// <summary>
        /// ID da configuração de distribuição utilizada
        /// </summary>
        public int ConfiguracaoDistribuicaoId { get; private set; }

        /// <summary>
        /// Data e hora da execução
        /// </summary>
        public DateTime DataExecucao { get; private set; }

        /// <summary>
        /// Total de leads que foram distribuídos nesta execução
        /// </summary>
        public int TotalLeadsDistribuidos { get; private set; }

        /// <summary>
        /// Total de vendedores que estavam ativos/elegíveis
        /// </summary>
        public int TotalVendedoresAtivos { get; private set; }

        /// <summary>
        /// JSON com o resultado detalhado da distribuição
        /// </summary>
        public string ResultadoDistribuicao { get; private set; }

        /// <summary>
        /// JSON com erros ocorridos durante a execução (se houver)
        /// </summary>
        public string ErrosOcorridos { get; private set; }

        /// <summary>
        /// Tempo total de execução do processo em segundos
        /// </summary>
        public int TempoExecucaoSegundos { get; private set; }

        /// <summary>
        /// ID do usuário que executou (null para execuções automáticas)
        /// </summary>
        public int? UsuarioExecutouId { get; private set; }

        // Propriedades de navegação
        public virtual ConfiguracaoDistribuicao ConfiguracaoDistribuicao { get; private set; }
        public virtual Usuario.Usuario? UsuarioExecutou { get; private set; }

        // Construtor para EF Core
        protected HistoricoDistribuicao() { }

        /// <summary>
        /// Cria um novo registro de histórico
        /// </summary>
        public HistoricoDistribuicao(
            int configuracaoDistribuicaoId,
            DateTime dataExecucao,
            int totalLeadsDistribuidos = 0,
            int totalVendedoresAtivos = 0,
            int tempoExecucaoSegundos = 0,
            int? usuarioExecutouId = null,
            string? resultadoDistribuicao = null,
            string? errosOcorridos = null)
        {
            if (configuracaoDistribuicaoId <= 0)
                throw new DomainException("ID da configuração deve ser maior que zero", nameof(HistoricoDistribuicao));

            ConfiguracaoDistribuicaoId = configuracaoDistribuicaoId;
            DataExecucao = dataExecucao;
            TotalLeadsDistribuidos = totalLeadsDistribuidos;
            TotalVendedoresAtivos = totalVendedoresAtivos;
            TempoExecucaoSegundos = tempoExecucaoSegundos;
            UsuarioExecutouId = usuarioExecutouId;
            ResultadoDistribuicao = resultadoDistribuicao ?? "{}";
            ErrosOcorridos = errosOcorridos ?? "[]";
        }

        /// <summary>
        /// Construtor alternativo que cria um registro de histórico para uma execução em andamento
        /// </summary>
        /// <param name="configuracaoDistribuicaoId">ID da configuração de distribuição</param>
        /// <param name="usuarioExecutouId">ID do usuário que executou</param>
        public HistoricoDistribuicao(
            int configuracaoDistribuicaoId,
            int? usuarioExecutouId = null)
        {
            if (configuracaoDistribuicaoId <= 0)
                throw new DomainException("ID da configuração deve ser maior que zero", nameof(HistoricoDistribuicao));

            ConfiguracaoDistribuicaoId = configuracaoDistribuicaoId;
            DataExecucao = TimeHelper.GetBrasiliaTime();
            TotalLeadsDistribuidos = 0;
            TotalVendedoresAtivos = 0;
            TempoExecucaoSegundos = 0;
            UsuarioExecutouId = usuarioExecutouId;
            ResultadoDistribuicao = "Distribuição em andamento";
            ErrosOcorridos = string.Empty;
        }

        /// <summary>
        /// Atualiza o resultado da distribuição
        /// </summary>
        /// <param name="totalLeadsDistribuidos">Total de leads distribuídos</param>
        /// <param name="totalVendedoresAtivos">Total de vendedores ativos</param>
        /// <param name="resultado">Resultado detalhado</param>
        /// <param name="erros">Erros ocorridos (opcional)</param>
        public void AtualizarResultado(
            int totalLeadsDistribuidos,
            int totalVendedoresAtivos,
            string resultado,
            string? erros = null)
        {
            TotalLeadsDistribuidos = totalLeadsDistribuidos;
            TotalVendedoresAtivos = totalVendedoresAtivos;
            ResultadoDistribuicao = resultado;
            
            if (!string.IsNullOrEmpty(erros))
            {
                ErrosOcorridos = erros;
            }
            
            // Calcula o tempo de execução
            var tempoExecucao = (int)(TimeHelper.GetBrasiliaTime() - DataExecucao).TotalSeconds;
            TempoExecucaoSegundos = Math.Max(0, tempoExecucao);
            
            // Atualiza data de modificação
            AtualizarDataModificacao();
        }

        /// <summary>
        /// Calcula a taxa de sucesso da distribuição (percentual de leads distribuídos)
        /// </summary>
        /// <param name="totalLeadsDisponiveis">Total de leads que estavam disponíveis</param>
        /// <returns>Taxa de sucesso em percentual</returns>
        public decimal CalcularTaxaSucesso(int totalLeadsDisponiveis)
        {
            if (totalLeadsDisponiveis <= 0)
                return 0;
                
            return Math.Round((decimal)TotalLeadsDistribuidos / totalLeadsDisponiveis * 100, 2);
        }
    }
}