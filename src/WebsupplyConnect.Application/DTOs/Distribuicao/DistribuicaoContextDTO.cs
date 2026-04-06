namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// DTO que contém o contexto necessário para o cálculo de distribuição
    /// </summary>
    public class DistribuicaoContextDTO
    {
        /// <summary>
        /// ID do lead sendo distribuído
        /// </summary>
        public int LeadId { get; set; }
        
        /// <summary>
        /// ID da empresa do lead
        /// </summary>
        public int EmpresaId { get; set; }
        
        /// <summary>
        /// ID do vendedor sendo avaliado
        /// </summary>
        public int VendedorId { get; set; }
        
        /// <summary>
        /// Dados da posição na fila (se aplicável)
        /// </summary>
        public FilaDistribuicaoDTO? PosicaoFila { get; set; }
        
        /// <summary>
        /// Métricas do vendedor (se aplicáveis)
        /// </summary>
        public MetricaVendedorDTO? MetricaVendedor { get; set; }
    }
    
    /// <summary>
    /// DTO com dados da posição na fila
    /// </summary>
    public class FilaDistribuicaoDTO
    {
        public int PosicaoFila { get; set; }
        public DateTime? DataUltimoLeadRecebido { get; set; }
        public int StatusFilaDistribuicaoId { get; set; }
        public bool PermiteRecebimento { get; set; }
    }
    
    /// <summary>
    /// DTO com métricas do vendedor
    /// </summary>
    public class MetricaVendedorDTO
    {
        public decimal TaxaConversao { get; set; }
        public decimal VelocidadeAtendimento { get; set; }
        public decimal TaxaPerdaInatividade { get; set; }
        public int QuantidadeLeadsAtivos { get; set; }
    }
}
