namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// DTO para score de vendedor em uma distribuição
    /// </summary>
    public class ScoreVendedorDTO
    {
        /// <summary>
        /// ID do vendedor
        /// </summary>
        public int VendedorId { get; set; }
        
        /// <summary>
        /// Nome do vendedor
        /// </summary>
        public string? NomeVendedor { get; set; }
        
        /// <summary>
        /// Score total calculado (0-100)
        /// </summary>
        public decimal ScoreTotal { get; set; }
        
        /// <summary>
        /// Posição no ranking (1 = melhor)
        /// </summary>
        public int Posicao { get; set; }
        
        /// <summary>
        /// Scores por regra
        /// </summary>
        public List<ScoreRegraDTO>? ScoresPorRegra { get; set; }
        
        /// <summary>
        /// Indica se o vendedor está elegível para receber o lead
        /// </summary>
        public bool Elegivel { get; set; }
        
        /// <summary>
        /// Motivo da inelegibilidade, se houver
        /// </summary>
        public string? MotivoInelegibilidade { get; set; }
        
        /// <summary>
        /// Número atual de leads ativos
        /// </summary>
        public int LeadsAtivos { get; set; }
        
        /// <summary>
        /// Taxa de conversão atual
        /// </summary>
        public decimal TaxaConversao { get; set; }
        
        /// <summary>
        /// Velocidade média de atendimento em minutos
        /// </summary>
        public decimal VelocidadeMediaAtendimento { get; set; }
        
        /// <summary>
        /// Posição atual na fila
        /// </summary>
        public int? PosicaoFila { get; set; }
    }
    
    /// <summary>
    /// DTO para score de uma regra específica
    /// </summary>
    public class ScoreRegraDTO
    {
        /// <summary>
        /// ID da regra
        /// </summary>
        public int RegraId { get; set; }
        
        /// <summary>
        /// Nome da regra
        /// </summary>
        public string? NomeRegra { get; set; }
        
        /// <summary>
        /// Tipo da regra (MERITO, FILA, TEMPO, etc.)
        /// </summary>
        public string? TipoRegra { get; set; }
        
        /// <summary>
        /// Score base calculado (0-100)
        /// </summary>
        public decimal ScoreBase { get; set; }
        
        /// <summary>
        /// Peso da regra (0-100)
        /// </summary>
        public decimal Peso { get; set; }
        
        /// <summary>
        /// Score ponderado (score base * peso / 100)
        /// </summary>
        public decimal ScoreComPeso { get; set; }
        
        /// <summary>
        /// Detalhes específicos do cálculo desta regra
        /// </summary>
        public Dictionary<string, object>? DetalhesCalculo { get; set; }
    }
}