using System.ComponentModel.DataAnnotations;

namespace WebsupplyConnect.Application.DTOs.Distribuicao
{
    /// <summary>
    /// DTO para vendedor na distribuição
    /// </summary>
    public class VendedorDistribuicaoDTO
    {
        /// <summary>
        /// ID do vendedor
        /// </summary>
        public int VendedorId { get; set; }
        
        /// <summary>
        /// Nome do vendedor
        /// </summary>
        public string NomeVendedor { get; set; } = string.Empty;
        
        /// <summary>
        /// Email do vendedor
        /// </summary>
        public string EmailVendedor { get; set; } = string.Empty;
        
        /// <summary>
        /// Indica se o vendedor está ativo na distribuição
        /// </summary>
        public bool AtivoDistribuicao { get; set; }
        
        /// <summary>
        /// Posição atual na fila de distribuição
        /// </summary>
        public int? PosicaoFila { get; set; }
        
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
        /// Score atual do vendedor
        /// </summary>
        public decimal ScoreAtual { get; set; }
        
        /// <summary>
        /// Data da última atribuição de lead
        /// </summary>
        public DateTime? DataUltimaAtribuicao { get; set; }
        
        /// <summary>
        /// Indica se o vendedor está disponível para receber leads
        /// </summary>
        public bool Disponivel { get; set; }
        
        /// <summary>
        /// Motivo da indisponibilidade, se houver
        /// </summary>
        public string? MotivoIndisponibilidade { get; set; }
    }
    
    /// <summary>
    /// DTO para configuração de vendedor na distribuição
    /// </summary>
    public class ConfigurarVendedorDistribuicaoDTO
    {
        /// <summary>
        /// ID do vendedor
        /// </summary>
        [Required(ErrorMessage = "ID do vendedor é obrigatório")]
        public int VendedorId { get; set; }
        
        /// <summary>
        /// Indica se o vendedor deve estar ativo na distribuição
        /// </summary>
        public bool AtivoDistribuicao { get; set; } = true;
        
        /// <summary>
        /// Posição desejada na fila (opcional)
        /// </summary>
        [Range(1, 1000, ErrorMessage = "Posição deve estar entre 1 e 1000")]
        public int? PosicaoFila { get; set; }
        
        /// <summary>
        /// Motivo da configuração
        /// </summary>
        [StringLength(500, ErrorMessage = "Motivo deve ter no máximo 500 caracteres")]
        public string? Motivo { get; set; }
    }
    
    /// <summary>
    /// DTO para configuração de horários de trabalho para distribuição
    /// </summary>
    public class ConfigurarHorariosDistribuicaoDTO
    {
        /// <summary>
        /// ID da configuração de distribuição
        /// </summary>
        [Required(ErrorMessage = "ID da configuração é obrigatório")]
        public int ConfiguracaoDistribuicaoId { get; set; }
        
        /// <summary>
        /// Indica se deve considerar horário de trabalho dos vendedores
        /// </summary>
        public bool ConsiderarHorarioTrabalho { get; set; } = true;
        
        /// <summary>
        /// Indica se deve considerar feriados e fins de semana
        /// </summary>
        public bool ConsiderarFeriados { get; set; } = true;
        
        /// <summary>
        /// Horário de início do expediente (HH:mm)
        /// </summary>
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Horário deve estar no formato HH:mm")]
        public string? HorarioInicioExpediente { get; set; }
        
        /// <summary>
        /// Horário de fim do expediente (HH:mm)
        /// </summary>
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Horário deve estar no formato HH:mm")]
        public string? HorarioFimExpediente { get; set; }
        
        /// <summary>
        /// Fuso horário (ex: "America/Sao_Paulo")
        /// </summary>
        [StringLength(50, ErrorMessage = "Fuso horário deve ter no máximo 50 caracteres")]
        public string? FusoHorario { get; set; }
        
        /// <summary>
        /// Configurações específicas por dia da semana
        /// </summary>
        public List<HorarioDiaSemanaDTO>? HorariosPorDia { get; set; }
    }
    
    /// <summary>
    /// DTO para horário de um dia específico da semana
    /// </summary>
    public class HorarioDiaSemanaDTO
    {
        /// <summary>
        /// ID do dia da semana (1=Segunda, 2=Terça, etc.)
        /// </summary>
        [Range(1, 7, ErrorMessage = "Dia da semana deve estar entre 1 e 7")]
        public int DiaSemanaId { get; set; }
        
        /// <summary>
        /// Nome do dia da semana
        /// </summary>
        public string NomeDia { get; set; } = string.Empty;
        
        /// <summary>
        /// Indica se o vendedor trabalha neste dia
        /// </summary>
        public bool TrabalhaNesteDia { get; set; } = true;
        
        /// <summary>
        /// Horário de início (HH:mm)
        /// </summary>
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Horário deve estar no formato HH:mm")]
        public string? HorarioInicio { get; set; }
        
        /// <summary>
        /// Horário de fim (HH:mm)
        /// </summary>
        [RegularExpression(@"^([01]?[0-9]|2[0-3]):[0-5][0-9]$", ErrorMessage = "Horário deve estar no formato HH:mm")]
        public string? HorarioFim { get; set; }
        
        /// <summary>
        /// Intervalo de almoço (em minutos)
        /// </summary>
        [Range(0, 240, ErrorMessage = "Intervalo de almoço deve estar entre 0 e 240 minutos")]
        public int? IntervaloAlmoco { get; set; }
    }
    
    /// <summary>
    /// DTO para resposta de configuração de vendedores
    /// </summary>
    public class ConfiguracaoVendedoresResponseDTO
    {
        /// <summary>
        /// ID da configuração de distribuição
        /// </summary>
        public int ConfiguracaoDistribuicaoId { get; set; }
        
        /// <summary>
        /// Total de vendedores configurados
        /// </summary>
        public int TotalVendedores { get; set; }
        
        /// <summary>
        /// Vendedores ativos na distribuição
        /// </summary>
        public int VendedoresAtivos { get; set; }
        
        /// <summary>
        /// Vendedores inativos na distribuição
        /// </summary>
        public int VendedoresInativos { get; set; }
        
        /// <summary>
        /// Lista de vendedores configurados
        /// </summary>
        public List<VendedorDistribuicaoDTO> Vendedores { get; set; } = new();
        
        /// <summary>
        /// Data da configuração
        /// </summary>
        public DateTime DataConfiguracao { get; set; }
    }
}
