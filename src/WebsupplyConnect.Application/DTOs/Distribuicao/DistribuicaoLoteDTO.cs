using System.ComponentModel.DataAnnotations;

namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// DTO para distribuição de leads em lote
    /// </summary>
    public class DistribuicaoLoteDTO
    {
        /// <summary>
        /// ID da empresa
        /// </summary>
        [Required]
        public int EmpresaId { get; set; }

        /// <summary>
        /// Lista de IDs dos leads a serem distribuídos
        /// </summary>
        [Required]
        [MinLength(1)]
        public List<int> LeadIds { get; set; } = new();

        /// <summary>
        /// Critérios específicos para distribuição
        /// </summary>
        public CriteriosDistribuicaoDTO? Criterios { get; set; }

        /// <summary>
        /// Indica se deve forçar redistribuição mesmo se já atribuído
        /// </summary>
        public bool ForcarRedistribuicao { get; set; } = false;

        /// <summary>
        /// Motivo da distribuição em lote
        /// </summary>
        [MaxLength(500)]
        public string? Motivo { get; set; }

        /// <summary>
        /// Indica se deve executar de forma síncrona ou assíncrona
        /// </summary>
        public bool ExecutarSincrono { get; set; } = true;
    }

    /// <summary>
    /// Critérios específicos para distribuição
    /// </summary>
    public class CriteriosDistribuicaoDTO
    {
        /// <summary>
        /// IDs específicos de vendedores para considerar
        /// </summary>
        public List<int>? VendedoresEspecificos { get; set; }

        /// <summary>
        /// Peso mínimo de score para considerar vendedor
        /// </summary>
        [Range(0, 100)]
        public decimal? ScoreMinimo { get; set; }

        /// <summary>
        /// Indica se deve considerar apenas vendedores ativos
        /// </summary>
        public bool ApenasVendedoresAtivos { get; set; } = true;

        /// <summary>
        /// Indica se deve considerar horário de trabalho
        /// </summary>
        public bool ConsiderarHorarioTrabalho { get; set; } = true;

        /// <summary>
        /// Indica se deve considerar carga atual do vendedor
        /// </summary>
        public bool ConsiderarCargaAtual { get; set; } = true;

        /// <summary>
        /// Máximo de leads ativos por vendedor
        /// </summary>
        [Range(1, 1000)]
        public int? MaxLeadsAtivosPorVendedor { get; set; }
    }

    /// <summary>
    /// Resultado da distribuição em lote
    /// </summary>
    public class DistribuicaoLoteResultadoDTO
    {
        /// <summary>
        /// ID da empresa
        /// </summary>
        public int EmpresaId { get; set; }

        /// <summary>
        /// Total de leads processados
        /// </summary>
        public int TotalLeadsProcessados { get; set; }

        /// <summary>
        /// Total de leads distribuídos com sucesso
        /// </summary>
        public int TotalLeadsDistribuidos { get; set; }

        /// <summary>
        /// Total de leads que falharam na distribuição
        /// </summary>
        public int TotalLeadsFalharam { get; set; }

        /// <summary>
        /// Tempo total de execução em segundos
        /// </summary>
        public decimal TempoExecucaoSegundos { get; set; }

        /// <summary>
        /// Detalhes das distribuições realizadas
        /// </summary>
        public List<DistribuicaoDetalheDTO> Distribuicoes { get; set; } = new();

        /// <summary>
        /// Erros encontrados durante a distribuição
        /// </summary>
        public List<ErroDistribuicaoDTO> Erros { get; set; } = new();

        /// <summary>
        /// Data e hora da execução
        /// </summary>
        public DateTime DataExecucao { get; set; } = DateTime.UtcNow;
    }

    /// <summary>
    /// Detalhes de uma distribuição específica
    /// </summary>
    public class DistribuicaoDetalheDTO
    {
        /// <summary>
        /// ID do lead
        /// </summary>
        public int LeadId { get; set; }

        /// <summary>
        /// ID do vendedor atribuído
        /// </summary>
        public int? VendedorId { get; set; }

        /// <summary>
        /// Nome do vendedor atribuído
        /// </summary>
        public string? NomeVendedor { get; set; }

        /// <summary>
        /// Score do vendedor atribuído
        /// </summary>
        public decimal? ScoreVendedor { get; set; }

        /// <summary>
        /// Status da distribuição
        /// </summary>
        public string Status { get; set; } = string.Empty;

        /// <summary>
        /// Mensagem de erro (se houver)
        /// </summary>
        public string? MensagemErro { get; set; }

        /// <summary>
        /// Tempo de processamento em milissegundos
        /// </summary>
        public decimal TempoProcessamentoMs { get; set; }
    }

    /// <summary>
    /// Erro específico de distribuição
    /// </summary>
    public class ErroDistribuicaoDTO
    {
        /// <summary>
        /// ID do lead que gerou erro
        /// </summary>
        public int LeadId { get; set; }

        /// <summary>
        /// Tipo do erro
        /// </summary>
        public string TipoErro { get; set; } = string.Empty;

        /// <summary>
        /// Mensagem de erro
        /// </summary>
        public string Mensagem { get; set; } = string.Empty;

        /// <summary>
        /// Código do erro
        /// </summary>
        public string? CodigoErro { get; set; }
    }
}
