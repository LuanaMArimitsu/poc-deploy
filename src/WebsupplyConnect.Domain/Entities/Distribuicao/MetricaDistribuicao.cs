using WebsupplyConnect.Domain.Entities.Base;
using WebsupplyConnect.Domain.Entities.Empresa;
using WebsupplyConnect.Domain.Exceptions;

namespace WebsupplyConnect.Domain.Entities.Distribuicao
{
    /// <summary>
    /// Armazena métricas consolidadas do sistema de distribuição por empresa e período.
    /// Utilizada para dashboards e análises gerenciais.
    /// </summary>
    public class MetricaDistribuicao : EntidadeBase
    {
        /// <summary>
        /// ID da empresa à qual as métricas pertencem
        /// </summary>
        public int EmpresaId { get; private set; }

        /// <summary>
        /// Data de referência das métricas (normalmente agregadas por dia)
        /// </summary>
        public DateTime DataReferencia { get; private set; }

        /// <summary>
        /// Total de leads recebidos no período
        /// </summary>
        public int TotalLeadsRecebidos { get; private set; }

        /// <summary>
        /// Total de leads que foram distribuídos com sucesso
        /// </summary>
        public int TotalLeadsDistribuidos { get; private set; }

        /// <summary>
        /// Total de leads que precisaram ser reatribuídos
        /// </summary>
        public int TotalReatribuicoes { get; private set; }

        /// <summary>
        /// Tempo médio de distribuição em segundos
        /// </summary>
        public decimal TempoMedioDistribuicao { get; private set; }

        /// <summary>
        /// Taxa de sucesso na distribuição (distribuídos/recebidos)
        /// </summary>
        public decimal TaxaSucessoDistribuicao { get; private set; }

        /// <summary>
        /// JSON com distribuição detalhada por vendedor
        /// </summary>
        public string DistribuicaoPorVendedor { get; private set; }

        /// <summary>
        /// JSON com distribuição por tipo de regra aplicada
        /// </summary>
        public string DistribuicaoPorRegra { get; private set; }

        // Propriedade de navegação
        public virtual Empresa.Empresa Empresa { get; private set; }

        // Construtor para EF Core
        protected MetricaDistribuicao() { }

        /// <summary>
        /// Cria uma nova métrica de distribuição
        /// </summary>
        public MetricaDistribuicao(
            int empresaId, 
            DateTime dataReferencia,
            int totalLeadsRecebidos = 0,
            int totalLeadsDistribuidos = 0,
            int totalReatribuicoes = 0,
            decimal tempoMedioDistribuicao = 0,
            decimal taxaSucessoDistribuicao = 0,
            string distribuicaoPorVendedor = null,
            string distribuicaoPorRegra = null)
        {
            if (empresaId <= 0)
                throw new DomainException("ID da empresa deve ser maior que zero", nameof(MetricaDistribuicao));

            EmpresaId = empresaId;
            DataReferencia = dataReferencia.Date; // Remove horário
            TotalLeadsRecebidos = totalLeadsRecebidos;
            TotalLeadsDistribuidos = totalLeadsDistribuidos;
            TotalReatribuicoes = totalReatribuicoes;
            TempoMedioDistribuicao = tempoMedioDistribuicao;
            TaxaSucessoDistribuicao = taxaSucessoDistribuicao;
            DistribuicaoPorVendedor = distribuicaoPorVendedor ?? "{}";
            DistribuicaoPorRegra = distribuicaoPorRegra ?? "{}";
        }
    }
}