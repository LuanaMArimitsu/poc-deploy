using System.ComponentModel.DataAnnotations;

namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// DTO para criação de uma configuração de distribuição
    /// </summary>
    public class ConfiguracaoDistribuicaoCriarDTO
    {
        /// <summary>
        /// Nome da configuração
        /// </summary>
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;
        
        /// <summary>
        /// Descrição da configuração
        /// </summary>
        [Required(ErrorMessage = "Descrição é obrigatória")]
        [StringLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres")]
        public string Descricao { get; set; } = string.Empty;
        
        /// <summary>
        /// ID da empresa a que pertence
        /// </summary>
        [Required(ErrorMessage = "ID da empresa é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "ID da empresa deve ser maior que zero")]
        public int EmpresaId { get; set; }
        
        /// <summary>
        /// Indica se a configuração será ativa
        /// </summary>
        public bool Ativa { get; set; }
        
        /// <summary>
        /// Data de início da vigência
        /// </summary>
        public DateTime? DataInicioVigencia { get; set; }
        
        /// <summary>
        /// Data de fim da vigência
        /// </summary>
        public DateTime? DataFimVigencia { get; set; }
        
        /// <summary>
        /// Número máximo de leads ativos por vendedor (SIMPLIFICADO)
        /// </summary>
        [Range(1, 100, ErrorMessage = "Número máximo de leads ativos deve estar entre 1 e 100")]
        public int MaxLeadsAtivosPorVendedor { get; set; } = 10;
        
        
        /// <summary>
        /// IDs das regras de distribuição associadas
        /// </summary>
        public List<int> RegrasDistribuicaoIds { get; set; } = new List<int>();
    }
    
    /// <summary>
    /// DTO para atualização de uma configuração de distribuição
    /// </summary>
    public class ConfiguracaoDistribuicaoAtualizarDTO
    {
        /// <summary>
        /// ID da configuração
        /// </summary>
        [Required(ErrorMessage = "ID é obrigatório")]
        [Range(1, int.MaxValue, ErrorMessage = "ID deve ser maior que zero")]
        public int Id { get; set; }
        
        /// <summary>
        /// Nome da configuração
        /// </summary>
        [Required(ErrorMessage = "Nome é obrigatório")]
        [StringLength(100, ErrorMessage = "Nome deve ter no máximo 100 caracteres")]
        public string Nome { get; set; } = string.Empty;
        
        /// <summary>
        /// Descrição da configuração
        /// </summary>
        [Required(ErrorMessage = "Descrição é obrigatória")]
        [StringLength(500, ErrorMessage = "Descrição deve ter no máximo 500 caracteres")]
        public string Descricao { get; set; } = string.Empty;
        
        /// <summary>
        /// Indica se a configuração será ativa
        /// </summary>
        public bool Ativa { get; set; }
        
        /// <summary>
        /// Data de início da vigência
        /// </summary>
        public DateTime? DataInicioVigencia { get; set; }
        
        /// <summary>
        /// Data de fim da vigência
        /// </summary>
        public DateTime? DataFimVigencia { get; set; }
        
        /// <summary>
        /// Número máximo de leads ativos por vendedor (SIMPLIFICADO)
        /// </summary>
        [Range(1, 100, ErrorMessage = "Número máximo de leads ativos deve estar entre 1 e 100")]
        public int MaxLeadsAtivosPorVendedor { get; set; } = 10;
        
        
        /// <summary>
        /// IDs das regras de distribuição associadas
        /// </summary>
        public List<int> RegrasDistribuicaoIds { get; set; } = new List<int>();
    }
    
    /// <summary>
    /// DTO para representação de uma configuração de distribuição
    /// </summary>
    public class ConfiguracaoDistribuicaoDTO
    {
        /// <summary>
        /// ID da configuração
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Nome da configuração
        /// </summary>
        public string Nome { get; set; } = string.Empty;
        
        /// <summary>
        /// Descrição da configuração
        /// </summary>
        public string Descricao { get; set; } = string.Empty;
        
        /// <summary>
        /// ID da empresa a que pertence
        /// </summary>
        public int EmpresaId { get; set; }
        
        /// <summary>
        /// Nome da empresa
        /// </summary>
        public string? NomeEmpresa { get; set; }
        
        /// <summary>
        /// Indica se a configuração está ativa
        /// </summary>
        public bool Ativa { get; set; }
        
        /// <summary>
        /// Data de início da vigência
        /// </summary>
        public DateTime? DataInicioVigencia { get; set; }
        
        /// <summary>
        /// Data de fim da vigência
        /// </summary>
        public DateTime? DataFimVigencia { get; set; }
        
        /// <summary>
        /// Número máximo de leads ativos por vendedor
        /// </summary>
        public int? MaxLeadsAtivosPorVendedor { get; set; }
        
        /// <summary>
        /// Intervalo mínimo entre atribuições ao mesmo vendedor (em minutos)
        /// </summary>
        public int? IntervaloMinimoEntreAtribuicoes { get; set; }
        
        /// <summary>
        /// Tempo máximo para primeiro atendimento (em minutos)
        /// </summary>
        public int? TempoMaximoPrimeiroAtendimento { get; set; }
        
        /// <summary>
        /// Indica se o sistema deve considerar horário de trabalho dos vendedores
        /// </summary>
        public bool ConsiderarHorarioTrabalho { get; set; }
        
        /// <summary>
        /// Data de criação
        /// </summary>
        public DateTime DataCriacao { get; set; }
        
        /// <summary>
        /// Data da última modificação
        /// </summary>
        public DateTime DataModificacao { get; set; }
        
        /// <summary>
        /// Lista de regras de distribuição associadas
        /// </summary>
        public List<RegraDistribuicaoDTO> Regras { get; set; } = new List<RegraDistribuicaoDTO>();
    }
    
    /// <summary>
    /// DTO para representação simplificada de uma regra de distribuição
    /// </summary>
    public class RegraDistribuicaoDTO
    {
        /// <summary>
        /// ID da regra
        /// </summary>
        public int Id { get; set; }
        
        /// <summary>
        /// Nome da regra
        /// </summary>
        public string Nome { get; set; } = string.Empty;
        
        /// <summary>
        /// Tipo da regra
        /// </summary>
        public string TipoRegra { get; set; } = string.Empty;
        
        /// <summary>
        /// Peso da regra na distribuição
        /// </summary>
        public decimal Peso { get; set; }
        
        /// <summary>
        /// Ordem de execução da regra
        /// </summary>
        public int Ordem { get; set; }
        
        /// <summary>
        /// Indica se a regra está ativa
        /// </summary>
        public bool Ativa { get; set; }
    }
}