namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// Estatísticas das regras de distribuição
    /// </summary>
    public class RegrasStatistics
    {
        /// <summary>
        /// Número total de regras ativas
        /// </summary>
        public int TotalRegras { get; set; }
        
        /// <summary>
        /// Soma total dos pesos das regras
        /// </summary>
        public decimal SomaPesos { get; set; }
        
        /// <summary>
        /// Número de tipos de regras distintos
        /// </summary>
        public int TiposRegrasDistintos { get; set; }
        
        /// <summary>
        /// Indica se há regras com ordem duplicada
        /// </summary>
        public bool TemOrdemDuplicada { get; set; }
        
        /// <summary>
        /// Número de regras que possuem parâmetros
        /// </summary>
        public int RegrasComParametros { get; set; }
        
        /// <summary>
        /// Menor peso entre as regras
        /// </summary>
        public decimal PesoMinimo { get; set; }
        
        /// <summary>
        /// Maior peso entre as regras
        /// </summary>
        public decimal PesoMaximo { get; set; }
        
        /// <summary>
        /// Indica se as regras estão balanceadas (soma = 100%)
        /// </summary>
        public bool PesosBalanceados => Math.Abs(SomaPesos - 100) <= 0.01m;
        
        /// <summary>
        /// Indica se há diversidade de tipos de regras
        /// </summary>
        public bool TemDiversidadeTipos => TiposRegrasDistintos > 1;
        
        /// <summary>
        /// Percentual de regras com parâmetros
        /// </summary>
        public decimal PercentualComParametros => TotalRegras > 0 ? (decimal)RegrasComParametros / TotalRegras * 100 : 0;
    }
}
